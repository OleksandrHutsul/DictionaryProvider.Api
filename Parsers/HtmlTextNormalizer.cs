using DictionaryProvider.Api.Configuration;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using System.Net;

namespace DictionaryProvider.Api.Parsers;

public class HtmlTextNormalizer
{
    private readonly Uri _cambridgeUri;

    public HtmlTextNormalizer(IOptions<CambridgeOptions> options)
    {
        _cambridgeUri = new Uri(options.Value.SiteUrl);
    }

    public string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var decoded = WebUtility.HtmlDecode(value);

        return string.Join(' ', decoded.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    public string? Text(HtmlNode? node)
    {
        return Clean(node?.InnerText);
    }

    public string? AbsoluteCambridgeUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri))
            return absoluteUri.ToString();

        return Uri.TryCreate(_cambridgeUri, value, out var uri)
            ? uri.ToString()
            : null;
    }
}
