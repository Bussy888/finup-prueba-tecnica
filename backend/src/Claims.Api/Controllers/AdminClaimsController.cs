using Claims.Application.Dtos;
using Claims.Application.Interfaces;
using Claims.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

[ApiController]
[Route("api/admin/claims")]
[Authorize(Policy = "Admin")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class AdminClaimsController(IClaimService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ClaimSummaryResponse>>> List(
        [FromQuery] ClaimStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default) =>
        await service.ListAsync(status, page, pageSize, ct);

    [HttpGet("{folio}")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimDetailResponse>> Get(string folio, CancellationToken ct) =>
        await service.GetDetailAsync(folio, ct);

    [HttpPatch("{folio}/status")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ClaimDetailResponse>> ChangeStatus(
        string folio, ChangeStatusRequest request, CancellationToken ct) =>
        await service.ChangeStatusAsync(folio, request, ct);
}
