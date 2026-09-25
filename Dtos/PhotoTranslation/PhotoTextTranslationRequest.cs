using DictionaryProvider.Api.Enums;

namespace DictionaryProvider.Api.Dtos.PhotoTranslation;

public class PhotoTextTranslationRequest
{
    public string Text { get; set; } = "";
    public PhotoTranslationMode Mode { get; set; } = PhotoTranslationMode.Text;
}
