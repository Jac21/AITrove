using System.Net.Http.Json;
using AITrove.Agents.Interfaces;
using AITrove.Context;
using AITrove.Prompts.Interfaces;
using AITrove.Workflows;

namespace AITrove.Agents.Implementations;

/// <summary>
/// A concrete IAgent implementation scoped to .NET code review.
/// Loads its system and user prompts from the Prompts/ folder at runtime,
/// keeping the model instructions fully decoupled from the C# code.
/// </summary>
public sealed class CodeReviewAgent(IPromptLoader prompts) : IAgent
{
    public string Name => "CodeReviewAgent";

    /// <summary>Only fires when the context carries a "code" metadata entry.</summary>
    public bool CanHandle(AgentContext ctx) =>
        ctx.Metadata.ContainsKey("code");

    public async Task<string> ExecuteAsync(AgentContext ctx, CancellationToken ct = default)
    {
        var code      = ctx.Metadata["code"].ToString() ?? string.Empty;
        var sysPrompt = await prompts.LoadAsync("code-review.system", ct);
        var userTmpl  = await prompts.LoadAsync("code-review.user", ct);
        var userMsg   = userTmpl.Replace("{{CODE}}", code);

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("x-api-key", ctx.ApiKey);
        http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var body = new
        {
            model                = "claude-sonnet-4-20250514",
            max_tokens_to_sample = 512,
            system               = sysPrompt,
            messages             = new[]
            {
                new
                {
                    role    = "user",
                    content = new[] { new { type = "text", text = userMsg } }
                }
            }
        };

        return await AnthropicClient.PostMessagesAsync(http, body, ct);
    }
}
