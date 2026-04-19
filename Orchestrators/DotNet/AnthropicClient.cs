using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AITrove;

internal static class AnthropicClient
{
    private const string MessagesUrl = "https://api.anthropic.com/v1/messages";

    public static async Task<string> PostMessagesAsync<TBody>(
        HttpClient http,
        TBody body,
        CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync(MessagesUrl, body, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var errorText = await resp.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Anthropic request failed {resp.StatusCode}: {errorText}");
        }

        return await ReadResponseTextAsync(resp.Content, ct);
    }

    private static async Task<string> ReadResponseTextAsync(HttpContent content, CancellationToken ct)
    {
        await using var stream = await content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        if (TryExtractText(doc.RootElement, out var text))
            return text.Trim();

        return string.Empty;
    }

    private static bool TryExtractText(JsonElement element, out string text)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                text = element.GetString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(text);

            case JsonValueKind.Array:
                var builder = new StringBuilder();
                foreach (var item in element.EnumerateArray())
                {
                    if (TryExtractText(item, out var itemText) && !string.IsNullOrWhiteSpace(itemText))
                        builder.Append(itemText);
                }

                text = builder.ToString();
                return !string.IsNullOrWhiteSpace(text);

            case JsonValueKind.Object:
                if (element.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
                {
                    text = textProp.GetString() ?? string.Empty;
                    return !string.IsNullOrWhiteSpace(text);
                }

                if (element.TryGetProperty("completion", out var completionProp) && TryExtractText(completionProp, out text))
                    return true;

                if (element.TryGetProperty("content", out var contentProp) && TryExtractText(contentProp, out text))
                    return true;

                if (element.TryGetProperty("choices", out var choicesProp) && choicesProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var choice in choicesProp.EnumerateArray())
                    {
                        if (TryExtractText(choice, out text) && !string.IsNullOrWhiteSpace(text))
                            return true;
                    }
                }

                if (element.TryGetProperty("message", out var messageProp) && TryExtractText(messageProp, out text))
                    return true;

                foreach (var property in element.EnumerateObject())
                {
                    if (TryExtractText(property.Value, out text) && !string.IsNullOrWhiteSpace(text))
                        return true;
                }

                break;
        }

        text = string.Empty;
        return false;
    }
}
