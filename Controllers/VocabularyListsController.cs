using System.Security.Claims;
using DictionaryProvider.Api.Dtos.Learning;
using DictionaryProvider.Api.Services.VocabularyList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DictionaryProvider.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/lists")]
public class VocabularyListsController : ControllerBase
{
    private readonly IVocabularyListService _vocabularyListService;

    public VocabularyListsController(IVocabularyListService vocabularyListService)
    {
        _vocabularyListService = vocabularyListService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VocabularyListSummaryDto>>> GetListsAsync([FromQuery] bool archived, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var lists = await _vocabularyListService.GetOwnedListsAsync(userId, archived, cancellationToken);
        return Ok(lists);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VocabularyListDetailDto>> GetListAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var list = await _vocabularyListService.GetListAsync(id, userId, cancellationToken);
        return list is null ? NotFound() : Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<VocabularyListDetailDto>> CreateListAsync(CreateVocabularyListRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        try
        {
            var list = await _vocabularyListService.CreateListAsync(userId, request, cancellationToken);
            return Created($"/api/lists/{list.Id}", list);
        }
        catch (InvalidOperationException exception)
        {
            return ValidationProblem(exception.Message);
        }
        catch (DbUpdateException)
        {
            return Problem("The vocabulary list could not be saved. Please try again after the database is ready.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VocabularyListDetailDto>> UpdateListAsync(Guid id, UpdateVocabularyListRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        try
        {
            var list = await _vocabularyListService.UpdateListAsync(id, userId, request, cancellationToken);
            return list is null ? NotFound() : Ok(list);
        }
        catch (InvalidOperationException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteListAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var deleted = await _vocabularyListService.DeleteListAsync(id, userId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> ArchiveListAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var archived = await _vocabularyListService.SetArchivedAsync(id, userId, true, cancellationToken);
        return archived ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> RestoreListAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var restored = await _vocabularyListService.SetArchivedAsync(id, userId, false, cancellationToken);
        return restored ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/test-results")]
    public async Task<IActionResult> SaveTestResultAsync(Guid id, SaveListTestResultRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        try
        {
            var saved = await _vocabularyListService.SaveTestResultAsync(id, userId, request, cancellationToken);
            return saved ? NoContent() : NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/words")]
    public async Task<ActionResult<VocabularyListWordDto>> AddWordAsync(Guid id, AddVocabularyWordRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        try
        {
            var word = await _vocabularyListService.AddWordAsync(id, userId, request, cancellationToken);
            return word is null ? NotFound() : Ok(word);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}/words/{wordId:guid}")]
    public async Task<IActionResult> RemoveWordAsync(Guid id, Guid wordId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var removed = await _vocabularyListService.RemoveWordAsync(id, wordId, userId, cancellationToken);
        return removed ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/share")]
    public async Task<ActionResult<VocabularyListShareDto>> ShareListAsync(Guid id, ShareVocabularyListRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var share = await _vocabularyListService.ShareListAsync(id, userId, request, cancellationToken);
        return share is null ? NotFound() : Ok(share);
    }

    [HttpDelete("{id:guid}/share/{userId:guid}")]
    public async Task<IActionResult> RemoveShareAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId)) return Unauthorized();

        var removed = await _vocabularyListService.RemoveShareAsync(id, userId, currentUserId, cancellationToken);
        return removed ? NoContent() : NotFound();
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }
}
