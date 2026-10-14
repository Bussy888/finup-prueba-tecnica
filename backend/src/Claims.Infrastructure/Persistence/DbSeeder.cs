using Claims.Domain;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, TimeProvider clock, CancellationToken ct = default)
    {
        if (await db.Claims.AnyAsync(ct)) return;

        var now = clock.GetUtcNow().UtcDateTime;
        var year = now.Year;

        var claims = new List<Claim>
        {
            Build(1, "Ana Torrez Quispe", "4567890 LP", "ana.torrez@example.com", "71234567",
                ClaimType.Cobros, "Me cobraron una comisión de mantenimiento que no figura en mi contrato.",
                now.AddDays(-1), ClaimStatus.Recibido),
            Build(2, "Luis Mamani Choque", "3456789 CB", "luis.mamani@example.com", null,
                ClaimType.Producto, "La tarjeta de débito llegó bloqueada y no me dan una solución.",
                now.AddDays(-3), ClaimStatus.EnProceso, ("EnProceso", "Estamos revisando el caso con el área de tarjetas.")),
            Build(3, "Carla Rojas Vega", "5678901 SC", "carla.rojas@example.com", "69876543",
                ClaimType.Atencion, "Esperé más de dos horas en la sucursal y nadie me atendió.",
                now.AddDays(-6), ClaimStatus.Resuelto,
                ("EnProceso", "Derivado a la jefatura de la sucursal."),
                ("Resuelto", "Se presentaron disculpas y se reforzó el personal en ventanilla.")),
            Build(4, "Jorge Villca Condori", "2345678 OR", "jorge.villca@example.com", null,
                ClaimType.Otro, "Pedí un certificado de cuenta hace semanas y todavía no lo recibo.",
                now.AddDays(-4), ClaimStatus.Rechazado,
                ("Rechazado", "El certificado ya fue entregado el día de la solicitud.")),
            Build(5, "Marta Choque Flores", "6789012 PT", "marta.choque@example.com", "72345678",
                ClaimType.Cobros, "Se debitó dos veces la cuota de mi préstamo este mes.",
                now.AddDays(-15), ClaimStatus.EnProceso, ("EnProceso", "Se solicitó el detalle de movimientos.")),
            Build(6, "Pedro Aliaga Rivero", "7890123 TJ", "pedro.aliaga@example.com", "70123456",
                ClaimType.Producto, "La app móvil no me deja ver el saldo de mi caja de ahorros.",
                now.AddDays(-2), ClaimStatus.Recibido)
        };

        db.Claims.AddRange(claims);
        db.FolioCounters.Add(new FolioCounter { Year = year, LastNumber = claims.Count });
        await db.SaveChangesAsync(ct);

        Claim Build(int number, string name, string document, string email, string? phone,
            ClaimType type, string description, DateTime createdAt, ClaimStatus status,
            params (string To, string Comment)[] steps)
        {
            var claim = new Claim
            {
                Folio = $"REC-{year}-{number:D6}",
                FullName = name,
                DocumentId = document,
                Email = email,
                Phone = phone,
                ClaimType = type,
                Description = description,
                Status = ClaimStatus.Recibido,
                CreatedAt = createdAt,
                DueDate = BusinessDayCalculator.AddBusinessDays(createdAt, 5)
            };
            claim.History.Add(new ClaimStatusHistory
            {
                ToStatus = ClaimStatus.Recibido,
                Comment = "Reclamo registrado.",
                ChangedAt = createdAt
            });

            var at = createdAt;
            foreach (var (to, comment) in steps)
            {
                at = at.AddHours(20);
                var next = Enum.Parse<ClaimStatus>(to);
                claim.History.Add(new ClaimStatusHistory
                {
                    FromStatus = claim.Status,
                    ToStatus = next,
                    Comment = comment,
                    ChangedAt = at
                });
                claim.Status = next;
                if (ClaimStatusRules.IsClosed(next)) claim.ResolvedAt = at;
            }

            return claim;
        }
    }
}
