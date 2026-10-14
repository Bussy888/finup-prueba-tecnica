using Claims.Application.Dtos;
using Claims.Application.Services;
using Claims.Domain.Enums;
using Claims.Domain.Exceptions;
using Claims.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace Claims.Tests;

public class ClaimServiceTests
{
    private readonly InMemoryClaimRepository _repo = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 13, 14, 30, 0, TimeSpan.Zero));
    private readonly ClaimService _service;

    public ClaimServiceTests()
    {
        _service = new ClaimService(_repo, _clock, NullLogger<ClaimService>.Instance);
    }

    private static CreateClaimRequest NewRequest(string email = "maria@example.com") => new(
        "María Fernández Rojas", "1234567 LP", email, "71234567",
        ClaimType.Cobros, "Se me cobró una comisión que no corresponde según mi contrato.");

    [Fact]
    public async Task Crear_devuelve_folio_estado_recibido_y_fecha_limite()
    {
        var response = await _service.CreateAsync(NewRequest(), default);

        Assert.StartsWith("REC-2026-", response.Folio);
        Assert.Equal(ClaimStatus.Recibido, response.Status);
        Assert.Equal(new DateTime(2026, 10, 20, 14, 30, 0, DateTimeKind.Utc), response.DueDate);
        Assert.Single(_repo.Claims[0].History);
    }

    [Fact]
    public async Task Crear_con_datos_invalidos_no_guarda_nada()
    {
        await Assert.ThrowsAsync<ValidationFailedException>(
            () => _service.CreateAsync(NewRequest() with { FullName = "a" }, default));

        Assert.Empty(_repo.Claims);
    }

    [Fact]
    public async Task Seguimiento_con_email_correcto_devuelve_el_historial()
    {
        var created = await _service.CreateAsync(NewRequest(), default);

        var tracking = await _service.TrackAsync(created.Folio, "MARIA@example.com", default);

        Assert.Equal(created.Folio, tracking.Folio);
        Assert.Single(tracking.History);
    }

    [Theory]
    [InlineData("otra@example.com")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Seguimiento_con_email_incorrecto_da_404(string? email)
    {
        var created = await _service.CreateAsync(NewRequest(), default);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.TrackAsync(created.Folio, email, default));
    }

    [Fact]
    public async Task Seguimiento_de_folio_inexistente_da_el_mismo_error_que_email_incorrecto()
    {
        var created = await _service.CreateAsync(NewRequest(), default);

        var missing = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.TrackAsync("REC-2026-999999", "maria@example.com", default));
        var wrongEmail = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.TrackAsync(created.Folio, "otra@example.com", default));

        Assert.Equal(missing.Message, wrongEmail.Message);
    }

    [Fact]
    public async Task Cambiar_estado_registra_historial()
    {
        var created = await _service.CreateAsync(NewRequest(), default);

        var detail = await _service.ChangeStatusAsync(
            created.Folio, new ChangeStatusRequest(ClaimStatus.EnProceso, "Lo estamos revisando"), default);

        Assert.Equal(ClaimStatus.EnProceso, detail.Status);
        Assert.Equal(2, detail.History.Count);
        Assert.Equal(ClaimStatus.Recibido, detail.History[1].FromStatus);
        Assert.Equal(1, _repo.SaveCalls);
    }

    [Fact]
    public async Task Resolver_guarda_la_fecha_de_cierre()
    {
        var created = await _service.CreateAsync(NewRequest(), default);
        await _service.ChangeStatusAsync(created.Folio, new ChangeStatusRequest(ClaimStatus.EnProceso, null), default);
        _clock.Advance(TimeSpan.FromDays(1));

        var detail = await _service.ChangeStatusAsync(
            created.Folio, new ChangeStatusRequest(ClaimStatus.Resuelto, "Se devolvió el cobro"), default);

        Assert.Equal(_clock.GetUtcNow().UtcDateTime, detail.ResolvedAt);
    }

    [Fact]
    public async Task Rechazar_sin_comentario_da_422()
    {
        var created = await _service.CreateAsync(NewRequest(), default);

        var ex = await Assert.ThrowsAsync<ValidationFailedException>(() => _service.ChangeStatusAsync(
            created.Folio, new ChangeStatusRequest(ClaimStatus.Rechazado, "  "), default));

        Assert.Contains("comment", ex.Errors.Keys);
    }

    [Fact]
    public async Task Transicion_no_permitida_da_409_y_no_guarda()
    {
        var created = await _service.CreateAsync(NewRequest(), default);

        await Assert.ThrowsAsync<ConflictException>(() => _service.ChangeStatusAsync(
            created.Folio, new ChangeStatusRequest(ClaimStatus.Resuelto, "directo"), default));

        Assert.Equal(0, _repo.SaveCalls);
    }

    [Fact]
    public async Task Cambiar_estado_de_un_folio_inexistente_da_404()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ChangeStatusAsync(
            "REC-2026-000999", new ChangeStatusRequest(ClaimStatus.EnProceso, null), default));
    }

    [Fact]
    public async Task Listado_marca_vencidos_solo_los_no_cerrados()
    {
        var open = await _service.CreateAsync(NewRequest(), default);
        var closed = await _service.CreateAsync(NewRequest(), default);
        await _service.ChangeStatusAsync(
            closed.Folio, new ChangeStatusRequest(ClaimStatus.Rechazado, "No procede"), default);
        _clock.Advance(TimeSpan.FromDays(10));

        var page = await _service.ListAsync(null, 1, 10, default);

        Assert.True(page.Items.Single(i => i.Folio == open.Folio).IsOverdue);
        Assert.False(page.Items.Single(i => i.Folio == closed.Folio).IsOverdue);
    }

    [Fact]
    public async Task Listado_no_marca_vencido_antes_de_la_fecha_limite()
    {
        await _service.CreateAsync(NewRequest(), default);
        _clock.Advance(TimeSpan.FromDays(3));

        var page = await _service.ListAsync(null, 1, 10, default);

        Assert.False(page.Items[0].IsOverdue);
    }

    [Fact]
    public async Task Listado_filtra_por_estado_y_pagina()
    {
        for (var i = 0; i < 3; i++) await _service.CreateAsync(NewRequest(), default);
        await _service.ChangeStatusAsync(
            _repo.Claims[0].Folio, new ChangeStatusRequest(ClaimStatus.EnProceso, null), default);

        var filtered = await _service.ListAsync(ClaimStatus.EnProceso, 1, 10, default);
        var paged = await _service.ListAsync(null, 2, 2, default);

        Assert.Single(filtered.Items);
        Assert.Single(paged.Items);
        Assert.Equal(3, paged.TotalCount);
        Assert.Equal(2, paged.TotalPages);
    }

    [Fact]
    public async Task Listado_corrige_pagina_y_tamano_fuera_de_rango()
    {
        await _service.CreateAsync(NewRequest(), default);

        var page = await _service.ListAsync(null, -4, 5000, default);

        Assert.Equal(1, page.Page);
        Assert.Equal(100, page.PageSize);
    }
}
