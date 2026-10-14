using Claims.Application.Dtos;
using Claims.Application.Interfaces;
using Claims.Application.Validation;
using Claims.Domain;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Claims.Application.Services;

public class ClaimService(IClaimRepository repository, TimeProvider clock, ILogger<ClaimService> logger)
    : IClaimService
{
    public const int DueBusinessDays = 5;
    private const int MaxPageSize = 100;

    public async Task<CreateClaimResponse> CreateAsync(CreateClaimRequest request, CancellationToken ct)
    {
        ClaimValidator.Validate(request);

        var now = clock.GetUtcNow().UtcDateTime;
        var claim = new Claim
        {
            FullName = request.FullName!.Trim(),
            DocumentId = request.DocumentId!.Trim(),
            Email = request.Email!.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            ClaimType = request.ClaimType,
            Description = request.Description!.Trim(),
            Status = ClaimStatus.Recibido,
            CreatedAt = now,
            DueDate = BusinessDayCalculator.AddBusinessDays(now, DueBusinessDays),
            History =
            [
                new ClaimStatusHistory
                {
                    ToStatus = ClaimStatus.Recibido,
                    Comment = "Reclamo registrado.",
                    ChangedAt = now
                }
            ]
        };

        await repository.AddAsync(claim, ct);
        logger.LogInformation("Reclamo {Folio} registrado ({ClaimType})", claim.Folio, claim.ClaimType);

        return new CreateClaimResponse(claim.Folio, claim.Status, claim.CreatedAt, claim.DueDate);
    }

    public async Task<ClaimTrackingResponse> TrackAsync(string folio, string? email, CancellationToken ct)
    {
        var claim = await repository.GetByFolioAsync(folio.Trim(), ct);

        var emailMatches = claim is not null && !string.IsNullOrWhiteSpace(email) &&
            string.Equals(claim.Email, email.Trim(), StringComparison.OrdinalIgnoreCase);
        if (!emailMatches)
            throw new NotFoundException("No se encontró un reclamo con esos datos.");

        var now = clock.GetUtcNow().UtcDateTime;
        return new ClaimTrackingResponse(
            claim!.Folio, claim.ClaimType, claim.Status, claim.CreatedAt, claim.DueDate,
            claim.ResolvedAt, claim.IsOverdue(now), MapHistory(claim));
    }

    public async Task<PagedResult<ClaimSummaryResponse>> ListAsync(
        ClaimStatus? status, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var (items, total) = await repository.ListAsync(status, (page - 1) * pageSize, pageSize, ct);
        var now = clock.GetUtcNow().UtcDateTime;

        var summaries = items
            .Select(c => new ClaimSummaryResponse(
                c.Folio, c.ClaimType, c.FullName, c.Status, c.CreatedAt, c.DueDate, c.IsOverdue(now)))
            .ToList();

        return new PagedResult<ClaimSummaryResponse>(summaries, page, pageSize, total);
    }

    public async Task<ClaimDetailResponse> GetDetailAsync(string folio, CancellationToken ct)
    {
        var claim = await FindOrThrowAsync(folio, ct);
        return MapDetail(claim, clock.GetUtcNow().UtcDateTime);
    }

    public async Task<ClaimDetailResponse> ChangeStatusAsync(
        string folio, ChangeStatusRequest request, CancellationToken ct)
    {
        var claim = await FindOrThrowAsync(folio, ct);
        ClaimValidator.Validate(request);

        if (!ClaimStatusRules.CanTransition(claim.Status, request.Status))
            throw new ConflictException($"No se puede pasar de {claim.Status} a {request.Status}.");

        var now = clock.GetUtcNow().UtcDateTime;
        var comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();

        claim.History.Add(new ClaimStatusHistory
        {
            ClaimId = claim.Id,
            FromStatus = claim.Status,
            ToStatus = request.Status,
            Comment = comment,
            ChangedAt = now
        });
        claim.Status = request.Status;
        claim.ResolvedAt = ClaimStatusRules.IsClosed(request.Status) ? now : null;

        await repository.SaveChangesAsync(ct);
        logger.LogInformation("Reclamo {Folio} pasó a {Status}", claim.Folio, claim.Status);

        return MapDetail(claim, now);
    }

    private async Task<Claim> FindOrThrowAsync(string folio, CancellationToken ct) =>
        await repository.GetByFolioAsync(folio.Trim(), ct)
        ?? throw new NotFoundException("No se encontró el reclamo.");

    private static List<StatusHistoryItem> MapHistory(Claim claim) =>
        claim.History
            .OrderBy(h => h.ChangedAt).ThenBy(h => h.Id)
            .Select(h => new StatusHistoryItem(h.FromStatus, h.ToStatus, h.Comment, h.ChangedAt))
            .ToList();

    private static ClaimDetailResponse MapDetail(Claim c, DateTime now) =>
        new(c.Folio, c.FullName, c.DocumentId, c.Email, c.Phone, c.ClaimType, c.Description,
            c.Status, c.CreatedAt, c.DueDate, c.ResolvedAt, c.IsOverdue(now), MapHistory(c));
}
