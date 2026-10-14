using Claims.Domain.Enums;

namespace Claims.Domain.Entities;

public class ClaimStatusHistory
{
    public int Id { get; set; }
    public int ClaimId { get; set; }
    public Claim? Claim { get; set; }

    public ClaimStatus? FromStatus { get; set; }
    public ClaimStatus ToStatus { get; set; }
    public string? Comment { get; set; }
    public DateTime ChangedAt { get; set; }
}
