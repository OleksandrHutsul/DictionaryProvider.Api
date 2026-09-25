using System.Security.Claims;
using DictionaryProvider.Api.Data;
using DictionaryProvider.Api.Dtos.Feedback;
using DictionaryProvider.Api.Entities;
using DictionaryProvider.Api.Services.FeedbackNotifier;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DictionaryProvider.Api.Controllers;

[ApiController]
[Route("api/feedback")]
[AllowAnonymous]
public class FeedbackController : ControllerBase
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Bug",
        "Incorrect translation",
        "Incorrect dictionary data",
        "Feature request",
        "Other"
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly IFeedbackNotifierService _feedbackNotifier;
    private readonly ILogger<FeedbackController> _logger;

    public FeedbackController(ApplicationDbContext dbContext, IFeedbackNotifierService feedbackNotifier, ILogger<FeedbackController> logger)
    {
        _dbContext = dbContext;
        _feedbackNotifier = feedbackNotifier;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitAsync([FromBody] FeedbackRequest request, CancellationToken cancellationToken)
    {
        var type = request.Type?.Trim() ?? string.Empty;
        var description = request.Description?.Trim() ?? string.Empty;

        if (!AllowedTypes.Contains(type))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid type",
                Detail = "Choose a valid report type."
            });
        }

        if (description.Length is < 10 or > 4000)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid description",
                Detail = "Description must be between 10 and 4000 characters."
            });
        }

        Guid? userId = null;
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdValue, out var parsedUserId))
            userId = parsedUserId;

        var report = new FeedbackReport
        {
            Type = type,
            Description = description,
            PageOrFeature = Truncate(request.PageOrFeature, 240),
            ContactEmail = Truncate(request.ContactEmail, 320),
            Diagnostics = Truncate(request.Diagnostics, 2000),
            UserId = userId
        };

        _dbContext.FeedbackReports.Add(report);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Feedback received. Id: {Id}. Type: {Type}. Page: {Page}. UserId: {UserId}", report.Id, report.Type, report.PageOrFeature, report.UserId);

        await _feedbackNotifier.NotifyNewReportAsync(report, cancellationToken);

        return Ok(new
        {
            id = report.Id,
            received = true
        });
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
