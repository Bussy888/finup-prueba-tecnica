using Claims.Application.Dtos;
using Claims.Application.Validation;
using Claims.Domain.Enums;
using Claims.Domain.Exceptions;

namespace Claims.Tests;

public class ClaimValidatorTests
{
    private static CreateClaimRequest Valid() => new(
        "María Fernández Rojas", "1234567 LP", "maria@example.com", "71234567",
        ClaimType.Cobros, "Se me cobró una comisión que no corresponde según mi contrato.");

    private static IDictionary<string, string[]> ErrorsOf(CreateClaimRequest request) =>
        Assert.Throws<ValidationFailedException>(() => ClaimValidator.Validate(request)).Errors;

    [Fact]
    public void Acepta_una_solicitud_valida()
    {
        ClaimValidator.Validate(Valid());
    }

    [Fact]
    public void El_telefono_es_opcional()
    {
        ClaimValidator.Validate(Valid() with { Phone = null });
        ClaimValidator.Validate(Valid() with { Phone = "" });
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Rechaza_nombres_cortos(string? name)
    {
        Assert.Contains("fullName", ErrorsOf(Valid() with { FullName = name }).Keys);
    }

    [Fact]
    public void Rechaza_nombres_de_mas_de_100_caracteres()
    {
        Assert.Contains("fullName", ErrorsOf(Valid() with { FullName = new string('a', 101) }).Keys);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void El_ci_es_obligatorio(string? document)
    {
        Assert.Contains("documentId", ErrorsOf(Valid() with { DocumentId = document }).Keys);
    }

    [Theory]
    [InlineData("maria")]
    [InlineData("maria@")]
    [InlineData("maria@example")]
    [InlineData("@example.com")]
    [InlineData(null)]
    public void Rechaza_emails_invalidos(string? email)
    {
        Assert.Contains("email", ErrorsOf(Valid() with { Email = email }).Keys);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("1234567890123456")]
    [InlineData("71234abc")]
    public void Rechaza_telefonos_invalidos(string phone)
    {
        Assert.Contains("phone", ErrorsOf(Valid() with { Phone = phone }).Keys);
    }

    [Theory]
    [InlineData("muy corta")]
    [InlineData(null)]
    public void Rechaza_descripciones_cortas(string? description)
    {
        Assert.Contains("description", ErrorsOf(Valid() with { Description = description }).Keys);
    }

    [Fact]
    public void Rechaza_descripciones_de_mas_de_2000_caracteres()
    {
        Assert.Contains("description", ErrorsOf(Valid() with { Description = new string('a', 2001) }).Keys);
    }

    [Fact]
    public void Reporta_todos_los_errores_juntos()
    {
        var errors = ErrorsOf(new CreateClaimRequest("a", "", "x", "1", ClaimType.Otro, "corta"));

        Assert.Equal(5, errors.Count);
    }
}
