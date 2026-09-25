using DictionaryProvider.Api.Configuration;
using DictionaryProvider.Api.Dtos.PhotoTranslation;
using DictionaryProvider.Api.Enums;
using DictionaryProvider.Api.Services.PhotoTranslation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DictionaryProvider.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/photo-translate")]
public class PhotoTranslateController : ControllerBase
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly IPhotoTranslationService _photoTranslationService;
    private readonly PhotoTranslationOptions _options;
    private readonly ILogger<PhotoTranslateController> _logger;

    public PhotoTranslateController(IPhotoTranslationService photoTranslationService, IOptions<PhotoTranslationOptions> options, ILogger<PhotoTranslateController> logger)
    {
        _photoTranslationService = photoTranslationService;
        _options = options.Value;
        _logger = logger;
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<PhotoTranslationDto>> TranslateAsync(IFormFile image, [FromForm] PhotoTranslationMode mode = PhotoTranslationMode.Text, CancellationToken cancellationToken = default)
    {
        if (image.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Empty image",
                Detail = "Choose an image that contains readable text."
            });
        }

        if (image.Length > _options.MaxImageBytes)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Image too large",
                Detail = "Images must be smaller than 10 MB."
            });
        }

        if (!AllowedTypes.Contains(image.ContentType))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unsupported image",
                Detail = "Use a JPG, PNG, or WebP image."
            });
        }

        try
        {
            await using var stream = image.OpenReadStream();

            var result = await _photoTranslationService.TranslateAsync(stream, image.FileName, image.ContentType, mode, cancellationToken);

            if (!string.IsNullOrWhiteSpace(result.TranslationError))
                _logger.LogWarning("Photo translate completed with translation error after OCR. Detail: {Detail}", result.TranslationError);

            return Ok(result);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception, "Photo translate failed before a recoverable result could be returned.");

            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Title = "Photo translation failed",
                Detail = exception.Message
            });
        }
    }

    [HttpPost("text")]
    public async Task<ActionResult<PhotoTranslationDto>> TranslateTextAsync([FromBody] PhotoTextTranslationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Empty text",
                Detail = "There is no recognized text to translate."
            });
        }

        try
        {
            var result = await _photoTranslationService.TranslateRecognizedTextAsync(request.Text, request.Mode, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception, "Photo text-only translation failed.");

            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Title = "Photo translation failed",
                Detail = exception.Message
            });
        }
    }
}
