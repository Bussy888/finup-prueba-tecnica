using Claims.Application.Interfaces;
using Claims.Domain.Entities;
using Claims.Domain.Enums;

namespace Claims.Tests.Fakes;

public class InMemoryClaimRepository : IClaimRepository
{
    public List<Claim> Claims { get; } = [];
    public int SaveCalls { get; private set; }

    public Task AddAsync(Claim claim, CancellationToken ct)
    {
        claim.Id = Claims.Count + 1;
        claim.Folio = $"REC-2026-{claim.Id:D6}";
        Claims.Add(claim);
        return Task.CompletedTask;
    }

    public Task<Claim?> GetByFolioAsync(string folio, CancellationToken ct) =>
        Task.FromResult(Claims.FirstOrDefault(c => c.Folio == folio));

    public Task<(IReadOnlyList<Claim> Items, int TotalCount)> ListAsync(
        ClaimStatus? status, int skip, int take, CancellationToken ct)
    {
        var filtered = Claims.Where(c => status is null || c.Status == status).ToList();
        IReadOnlyList<Claim> page = filtered.Skip(skip).Take(take).ToList();
        return Task.FromResult((page, filtered.Count));
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        SaveCalls++;
        return Task.CompletedTask;
    }
}
