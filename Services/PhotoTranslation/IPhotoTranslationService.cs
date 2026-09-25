using DictionaryProvider.Api.Dtos.PhotoTranslation;
using DictionaryProvider.Api.Enums;

namespace DictionaryProvider.Api.Services.PhotoTranslation;

public interface IPhotoTranslationService
{
    Task<PhotoTranslationDto> TranslateAsync(Stream image, string fileName, string contentType, PhotoTranslationMode mode, CancellationToken cancellationToken);
    Task<PhotoTranslationDto> TranslateRecognizedTextAsync(string originalText, PhotoTranslationMode mode, CancellationToken cancellationToken);
}