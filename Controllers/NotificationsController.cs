using System.Security.Claims;
using DictionaryProvider.Api.Data;
using DictionaryProvider.Api.Dtos.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DictionaryProvider.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public NotificationsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetAsync(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var notifications = await _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.RecipientUserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(100)
            .Select(notification => new NotificationDto
            {
                Id = notification.Id,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                RelatedListId = notification.RelatedListId,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead
            })
            .ToListAsync(cancellationToken);

        return Ok(notifications);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkReadAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var now = DateTimeOffset.UtcNow;

        var updated = await _dbContext.Notifications
            .Where(notification => notification.Id == id && notification.RecipientUserId == userId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(notification => notification.IsRead, true)
                .SetProperty(notification => notification.ReadAt, now), cancellationToken);

        return updated > 0 ? NoContent() : NotFound();
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }
}
