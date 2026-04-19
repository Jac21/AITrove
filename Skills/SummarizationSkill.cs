/// SummarizationSkill.cs
/// Summarizes arbitrary text to a target word count using Claude Haiku.
/// Use Haiku for this — it's cheap, fast, and more than capable enough.

namespace AITrove.Skills;

public static class SummarizationSkill
{
    public static async Task<string> SummarizeAsync(
        string text,
        string apiKey,
        int    maxWords = 120,
        CancellationToken ct = default)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("x-api-key", apiKey);
        http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var body = new
        {
            model                = "claude-haiku-4-5-20251001",
            max_tokens_to_sample = 256,
            system               = "Summarize clearly and concisely.",
            messages             = new[]
            {
                new
                {
                    role    = "user",
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = $"Summarize the following in {maxWords} words or fewer. " +
                                   $"Return only the summary — no preamble.\n\n{text}"
                        }
                    }
                }
            }
        };

        var resp = await http.PostAsJsonAsync("https://api.anthropic.com/v1/messages", body, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadFromJsonAsync<AnthropicResponse>(cancellationToken: ct);
        return json?.Content?[0].Text ?? string.Empty;
    }
}
