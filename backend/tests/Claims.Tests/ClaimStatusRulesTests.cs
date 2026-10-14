using Claims.Domain;
using Claims.Domain.Enums;

namespace Claims.Tests;

public class ClaimStatusRulesTests
{
    [Theory]
    [InlineData(ClaimStatus.Recibido, ClaimStatus.EnProceso)]
    [InlineData(ClaimStatus.Recibido, ClaimStatus.Rechazado)]
    [InlineData(ClaimStatus.EnProceso, ClaimStatus.Resuelto)]
    [InlineData(ClaimStatus.EnProceso, ClaimStatus.Rechazado)]
    public void Permite_las_transiciones_validas(ClaimStatus from, ClaimStatus to)
    {
        Assert.True(ClaimStatusRules.CanTransition(from, to));
    }

    [Theory]
    [InlineData(ClaimStatus.Recibido, ClaimStatus.Resuelto)]
    [InlineData(ClaimStatus.Recibido, ClaimStatus.Recibido)]
    [InlineData(ClaimStatus.EnProceso, ClaimStatus.Recibido)]
    [InlineData(ClaimStatus.Resuelto, ClaimStatus.EnProceso)]
    [InlineData(ClaimStatus.Resuelto, ClaimStatus.Rechazado)]
    [InlineData(ClaimStatus.Rechazado, ClaimStatus.EnProceso)]
    [InlineData(ClaimStatus.Rechazado, ClaimStatus.Resuelto)]
    public void Rechaza_las_demas_transiciones(ClaimStatus from, ClaimStatus to)
    {
        Assert.False(ClaimStatusRules.CanTransition(from, to));
    }

    [Theory]
    [InlineData(ClaimStatus.Resuelto, true)]
    [InlineData(ClaimStatus.Rechazado, true)]
    [InlineData(ClaimStatus.EnProceso, false)]
    [InlineData(ClaimStatus.Recibido, false)]
    public void Cerrar_exige_comentario(ClaimStatus to, bool required)
    {
        Assert.Equal(required, ClaimStatusRules.RequiresComment(to));
    }
}
