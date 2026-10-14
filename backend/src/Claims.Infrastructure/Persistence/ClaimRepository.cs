using Claims.Application.Interfaces;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Persistence;

public class ClaimRepository(AppDbContext db, TimeProvider clock) : IClaimRepository
{
    public async Task AddAsync(Claim claim, CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            var year = clock.GetUtcNow().Year;
            var number = await NextNumberAsync(year, ct);
            claim.Folio = $"REC-{year}-{number:D6}";

            db.Claims.Add(claim);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });
    }

    public Task<Claim?> GetByFolioAsync(string folio, CancellationToken ct) =>
        db.Claims.Include(c => c.History).FirstOrDefaultAsync(c => c.Folio == folio, ct);

    public async Task<(IReadOnlyList<Claim> Items, int TotalCount)> ListAsync(
        ClaimStatus? status, int skip, int take, CancellationToken ct)
    {
        var query = db.Claims.AsNoTracking();
        if (status is not null)
            query = query.Where(c => c.Status == status);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Id)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("El reclamo fue modificado por otra persona. Vuelve a cargarlo e intenta de nuevo.");
        }
    }

    // El MERGE con HOLDLOCK toma el lock de la fila del año: dos solicitudes
    // simultáneas no pueden recibir el mismo número.
    private async Task<int> NextNumberAsync(int year, CancellationToken ct)
    {
        var result = await db.Database
            .SqlQuery<int>($"""
                MERGE FolioCounters WITH (HOLDLOCK) AS target
                USING (SELECT {year} AS Year) AS source ON target.Year = source.Year
                WHEN MATCHED THEN UPDATE SET LastNumber = target.LastNumber + 1
                WHEN NOT MATCHED THEN INSERT (Year, LastNumber) VALUES ({year}, 1)
                OUTPUT inserted.LastNumber AS Value;
                """)
            .ToListAsync(ct);

        return result.Single();
    }
}
