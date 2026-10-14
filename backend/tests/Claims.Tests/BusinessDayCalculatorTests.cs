using Claims.Domain;

namespace Claims.Tests;

public class BusinessDayCalculatorTests
{
    [Theory]
    [InlineData("2026-10-12", "2026-10-19")]
    [InlineData("2026-10-14", "2026-10-21")]
    [InlineData("2026-10-16", "2026-10-23")]
    public void Suma_cinco_dias_habiles_desde_un_dia_de_semana(string start, string expected)
    {
        var result = BusinessDayCalculator.AddBusinessDays(DateTime.Parse(start), 5);

        Assert.Equal(DateTime.Parse(expected), result);
    }

    [Theory]
    [InlineData("2026-10-17", "2026-10-23")]
    [InlineData("2026-10-18", "2026-10-23")]
    public void Si_el_registro_es_fin_de_semana_cuenta_desde_el_lunes(string start, string expected)
    {
        var result = BusinessDayCalculator.AddBusinessDays(DateTime.Parse(start), 5);

        Assert.Equal(DateTime.Parse(expected), result);
    }

    [Fact]
    public void Conserva_la_hora_del_registro()
    {
        var start = new DateTime(2026, 10, 13, 14, 30, 0, DateTimeKind.Utc);

        var result = BusinessDayCalculator.AddBusinessDays(start, 5);

        Assert.Equal(new DateTime(2026, 10, 20, 14, 30, 0, DateTimeKind.Utc), result);
    }

    [Fact]
    public void Cero_dias_devuelve_la_misma_fecha()
    {
        var start = new DateTime(2026, 10, 13);

        Assert.Equal(start, BusinessDayCalculator.AddBusinessDays(start, 0));
    }

    [Fact]
    public void Rechaza_dias_negativos()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BusinessDayCalculator.AddBusinessDays(DateTime.UtcNow, -1));
    }
}
