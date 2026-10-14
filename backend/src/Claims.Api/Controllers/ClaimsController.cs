using Claims.Application.Dtos;
using Claims.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

[ApiController]
[Route("api/claims")]
public class ClaimsController(IClaimService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateClaimResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(CreateClaimRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Track), new { folio = created.Folio }, created);
    }

    [HttpGet("{folio}")]
    [ProducesResponseType<ClaimTrackingResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimTrackingResponse>> Track(
        string folio, [FromQuery] string? email, CancellationToken ct) =>
        await service.TrackAsync(folio, email, ct);
}
