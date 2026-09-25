using DictionaryProvider.Api.Data;
using DictionaryProvider.Api.Dtos.Feedback;
using DictionaryProvider.Api.Entities;
using DictionaryProvider.Api.Enums;
using DictionaryProvider.Api.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DictionaryProvider.Api.Controllers;

[ApiController]
[Route("api/feedback/admin")]
[Authorize(Policy = AdminAuthorization.DeveloperPolicy)]
public class FeedbackAdminController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public FeedbackAdminController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FeedbackReportDto>>> ListAsync(CancellationToken cancellationToken)
    {
        var reports = await _dbContext.FeedbackReports
            .AsNoTracking()
            .Include(report => report.User)
            .OrderByDescending(report => report.CreatedAt)
            .Select(report => Map(report))
            .ToListAsync(cancellationToken);

        return Ok(reports);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FeedbackReportDto>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var report = await _dbContext.FeedbackReports
            .AsNoTracking()
            .Include(report => report.User)
            .SingleOrDefaultAsync(report => report.Id == id, cancellationToken);

        if (report is null) return NotFound();

        return Ok(Map(report));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<FeedbackReportDto>> UpdateStatusAsync(Guid id, [FromBody] UpdateFeedbackStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<FeedbackReportStatus>(request.Status?.Trim(), true, out var status) || !Enum.IsDefined(status))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid status",
                Detail = "Use New, InProgress, or Resolved."
            });
        }

        var report = await _dbContext.FeedbackReports
            .Include(report => report.User)
            .SingleOrDefaultAsync(report => report.Id == id, cancellationToken);

        if (report is null) return NotFound();

        report.Status = status;
        report.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(Map(report));
    }

    private static FeedbackReportDto Map(FeedbackReport report)
    {
        return new FeedbackReportDto
        {
            Id = report.Id,
            Type = report.Type,
            Description = report.Description,
            PageOrFeature = report.PageOrFeature,
            ContactEmail = report.ContactEmail,
            Diagnostics = report.Diagnostics,
            Status = report.Status.ToString(),
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt,
            UserId = report.UserId,
            UserEmail = report.User?.Email
        };
    }
}
