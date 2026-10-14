using Claims.Domain.Entities;
using Claims.Domain.Enums;

namespace Claims.Application.Interfaces;

public interface IClaimRepository
{
    Task AddAsync(Claim claim, CancellationToken ct);

    Task<Claim?> GetByFolioAsync(string folio, CancellationToken ct);

    Task<(IReadOnlyList<Claim> Items, int TotalCount)> ListAsync(
        ClaimStatus? status, int skip, int take, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
