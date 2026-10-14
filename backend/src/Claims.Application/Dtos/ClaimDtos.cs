using Claims.Domain.Enums;

namespace Claims.Application.Dtos;

public record CreateClaimRequest(
    string? FullName,
    string? DocumentId,
    string? Email,
    string? Phone,
    ClaimType ClaimType,
    string? Description);

public record CreateClaimResponse(string Folio, ClaimStatus Status, DateTime CreatedAt, DateTime DueDate);

public record ChangeStatusRequest(ClaimStatus Status, string? Comment);

public record StatusHistoryItem(ClaimStatus? FromStatus, ClaimStatus ToStatus, string? Comment, DateTime ChangedAt);

public record ClaimTrackingResponse(
    string Folio,
    ClaimType ClaimType,
    ClaimStatus Status,
    DateTime CreatedAt,
    DateTime DueDate,
    DateTime? ResolvedAt,
    bool IsOverdue,
    IReadOnlyList<StatusHistoryItem> History);

public record ClaimSummaryResponse(
    string Folio,
    ClaimType ClaimType,
    string FullName,
    ClaimStatus Status,
    DateTime CreatedAt,
    DateTime DueDate,
    bool IsOverdue);

public record ClaimDetailResponse(
    string Folio,
    string FullName,
    string DocumentId,
    string Email,
    string? Phone,
    ClaimType ClaimType,
    string Description,
    ClaimStatus Status,
    DateTime CreatedAt,
    DateTime DueDate,
    DateTime? ResolvedAt,
    bool IsOverdue,
    IReadOnlyList<StatusHistoryItem> History);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
