using System.Net.Http;
using Anthropic.SDK.Messaging;

namespace AITrove;

public sealed class AnthropicMessageClient : IAnthropicMessageClient
{
    public async Task<string> SendMessageAsync(
        string apiKey,
        string requestedModel,
        int maxTokens,
        string systemPrompt,
        string userMessage,
        CancellationToken ct = default)
    {
        Exception? lastModelNotFound = null;

        foreach (var model in AnthropicModelIds.ResolveModelCandidates(requestedModel))
        {
            try
            {
                return await SendSingleMessageAsync(apiKey, model, maxTokens, systemPrompt, userMessage, ct);
            }
            catch (HttpRequestException ex) when (IsModelNotFound(ex))
            {
                lastModelNotFound = ex;
            }
        }

        if (lastModelNotFound is not null)
        {
            var attemptedModels = string.Join(", ", AnthropicModelIds.ResolveModelCandidates(requestedModel));
            throw new HttpRequestException(
                $"Anthropic model lookup failed after trying: {attemptedModels}. " +
                $"Set {AnthropicModelIds.ModelOverrideEnvironmentVariable} to a model available to your account. " +
                $"Last error: {lastModelNotFound.Message}",
                lastModelNotFound);
        }

        throw new InvalidOperationException("No Anthropic model candidates were resolved.");
    }

    private static async Task<string> SendSingleMessageAsync(
        string apiKey,
        string model,
        int maxTokens,
        string systemPrompt,
        string userMessage,
        CancellationToken ct)
    {
        using var httpClient = CreateHttpClient();
        using var client = new global::Anthropic.SDK.AnthropicClient(apiKey, httpClient);

        var parameters = new MessageParameters
        {
            Model         = model,
            MaxTokens     = maxTokens,
            SystemMessage = systemPrompt,
            Messages      = [new Message(RoleType.User, userMessage)],
            Stream        = false
        };

        global::System.Collections.Generic.IList<global::Anthropic.SDK.Common.Tool> tools = [];
        var response = await client.Messages.GetClaudeMessageAsync(parameters, tools, ct);
        var text = response.Message?.ToString()?.Trim();
        if (!string.IsNullOrWhiteSpace(text))
            return text;

        return response.FirstMessage?.Text?.Trim() ?? string.Empty;
    }

    private static bool IsModelNotFound(HttpRequestException ex) =>
        ex.Message.Contains("\"type\":\"not_found_error\"", StringComparison.OrdinalIgnoreCase) &&
        ex.Message.Contains("model:", StringComparison.OrdinalIgnoreCase);

    private static HttpClient CreateHttpClient() =>
        new(new HttpClientHandler
        {
            UseCookies = false
        });
}
