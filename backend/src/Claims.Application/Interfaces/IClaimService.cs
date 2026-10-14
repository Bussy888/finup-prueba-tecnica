using Claims.Application.Dtos;
using Claims.Domain.Enums;

namespace Claims.Application.Interfaces;

public interface IClaimService
{
    Task<CreateClaimResponse> CreateAsync(CreateClaimRequest request, CancellationToken ct);
    Task<ClaimTrackingResponse> TrackAsync(string folio, string? email, CancellationToken ct);
    Task<PagedResult<ClaimSummaryResponse>> ListAsync(ClaimStatus? status, int page, int pageSize, CancellationToken ct);
    Task<ClaimDetailResponse> GetDetailAsync(string folio, CancellationToken ct);
    Task<ClaimDetailResponse> ChangeStatusAsync(string folio, ChangeStatusRequest request, CancellationToken ct);
}
