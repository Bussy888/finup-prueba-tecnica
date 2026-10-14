using Claims.Domain.Enums;

namespace Claims.Domain;

public static class ClaimStatusRules
{
    private static readonly Dictionary<ClaimStatus, ClaimStatus[]> Allowed = new()
    {
        [ClaimStatus.Recibido] = [ClaimStatus.EnProceso, ClaimStatus.Rechazado],
        [ClaimStatus.EnProceso] = [ClaimStatus.Resuelto, ClaimStatus.Rechazado],
        [ClaimStatus.Resuelto] = [],
        [ClaimStatus.Rechazado] = []
    };

    public static bool CanTransition(ClaimStatus from, ClaimStatus to) => Allowed[from].Contains(to);

    public static bool IsClosed(ClaimStatus status) => status is ClaimStatus.Resuelto or ClaimStatus.Rechazado;

    public static bool RequiresComment(ClaimStatus to) => IsClosed(to);
}
