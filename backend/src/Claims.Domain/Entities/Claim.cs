using Claims.Domain.Enums;

namespace Claims.Domain.Entities;

public class Claim
{
    public int Id { get; set; }
    public string Folio { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public ClaimType ClaimType { get; set; }
    public string Description { get; set; } = string.Empty;
    public ClaimStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public List<ClaimStatusHistory> History { get; set; } = [];

    public bool IsOverdue(DateTime utcNow) => !ClaimStatusRules.IsClosed(Status) && DueDate < utcNow;
}
