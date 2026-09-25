using DictionaryProvider.Api.Dtos.Dictionary;
using DictionaryProvider.Api.Services.Dictionary;
using DictionaryProvider.Api.Services.DictionaryLookup;
using Microsoft.AspNetCore.Mvc;

namespace DictionaryProvider.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DictionaryController : ControllerBase
{
    private readonly IDictionaryService _dictionaryService;
    private readonly IDictionaryLookupService _dictionaryLookupService;
    private readonly ILogger<DictionaryController> _logger;
    private readonly IHostEnvironment _environment;

    public DictionaryController(IDictionaryService dictionaryService, IDictionaryLookupService dictionaryLookupService, ILogger<DictionaryController> logger, 
        IHostEnvironment environment)
    {
        _dictionaryService = dictionaryService;
        _dictionaryLookupService = dictionaryLookupService;
        _logger = logger;
        _environment = environment;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchAsync([FromQuery] string? query, [FromQuery] string? term, CancellationToken cancellationToken)
    {
        var searchTerm = !string.IsNullOrWhiteSpace(query) ? query : term;
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Trim().Length < 2) return Ok(Array.Empty<DictionarySuggestionDto>());

        try
        {
            var suggestions = await _dictionaryService.SearchAsync(searchTerm, cancellationToken);
            return Ok(suggestions);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Dictionary search failed for '{Query}'.", searchTerm);

            var detail = _environment.IsDevelopment()
                ? exception.Message
                : "Dictionary search failed.";

            return Problem(detail, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<DictionaryLookupResultDto>> LookupAsync([FromQuery] string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest();

        var result = await _dictionaryLookupService.LookupAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("suggestion")]
    public async Task<ActionResult<DictionaryWordDto>> GetSuggestionAsync([FromBody] DictionarySuggestionDto suggestion, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(suggestion.Word) || string.IsNullOrWhiteSpace(suggestion.Url)) return BadRequest();

        var result = await _dictionaryService.GetSuggestionAsync(suggestion, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{word}")]
    public async Task<IActionResult> GetWordAsync(string word, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _dictionaryService.GetWordAsync(word, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Dictionary parsing failed for word '{Word}'.", word);

            var detail = _environment.IsDevelopment()
                ? exception.Message
                : "Dictionary parsing failed.";

            return Problem(detail, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
