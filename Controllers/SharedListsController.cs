using System.Security.Claims;
using DictionaryProvider.Api.Dtos.Learning;
using DictionaryProvider.Api.Services.VocabularyList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DictionaryProvider.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/shared-lists")]
public class SharedListsController : ControllerBase
{
    private readonly IVocabularyListService _vocabularyListService;

    public SharedListsController(IVocabularyListService vocabularyListService)
    {
        _vocabularyListService = vocabularyListService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VocabularyListSummaryDto>>> GetSharedListsAsync(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var lists = await _vocabularyListService.GetSharedListsAsync(userId, cancellationToken);
        return Ok(lists);
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }
}
