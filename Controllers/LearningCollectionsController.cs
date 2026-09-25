using System.Security.Claims;
using DictionaryProvider.Api.Dtos.Learning;
using DictionaryProvider.Api.Services.LearningCollections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DictionaryProvider.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/learning-collections")]
public class LearningCollectionsController : ControllerBase
{
    private readonly ILearningCollectionsService _learningCollectionsService;
    private readonly ILogger<LearningCollectionsController> _logger;

    public LearningCollectionsController(ILearningCollectionsService learningCollectionsService, ILogger<LearningCollectionsController> logger)
    {
        _learningCollectionsService = learningCollectionsService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<LearningCollectionsSnapshotDto>> GetAsync(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var snapshot = await _learningCollectionsService.GetAsync(userId, cancellationToken);
        return Ok(snapshot);
    }

    [HttpPut]
    public async Task<ActionResult<LearningCollectionsSnapshotDto>> SaveAsync(SaveLearningCollectionsRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        try
        {
            var snapshot = await _learningCollectionsService.SaveAsync(userId, request, cancellationToken);
            return Ok(snapshot);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Failed to save learning collections for user {UserId}.", userId);
            return Problem("Learning collections could not be saved to the database.");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        try
        {
            var deleted = await _learningCollectionsService.DeleteAsync(userId, id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Failed to delete learning collection {CollectionId} for user {UserId}.", id, userId);
            return Problem("The collection could not be removed from the database.");
        }
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }
}
