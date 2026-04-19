namespace AITrove;

public interface IAnthropicMessageClient
{
    Task<string> SendMessageAsync(
        string apiKey,
        string model,
        int maxTokens,
        string systemPrompt,
        string userMessage,
        CancellationToken ct = default);
}

public static class AnthropicModelIds
{
    public const string ModelOverrideEnvironmentVariable = "AITROVE_ANTHROPIC_MODEL";

    // Anthropic recommends versioned model IDs in production for predictable behavior.
    public const string ClaudeSonnet4 = "claude-sonnet-4-20250514";
    public const string ClaudeSonnet4Alias = "claude-sonnet-4-0";
    public const string ClaudeSonnet37Latest = "claude-sonnet-4-6";
    public const string ClaudeSonnet35Latest = "claude-3-5-sonnet-latest";

    public static IReadOnlyList<string> ResolveModelCandidates(string requestedModel)
    {
        var overrideModel = Environment.GetEnvironmentVariable(ModelOverrideEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(overrideModel))
            return [overrideModel.Trim()];

        return requestedModel switch
        {
            ClaudeSonnet4 => [ClaudeSonnet4, ClaudeSonnet4Alias, ClaudeSonnet37Latest, ClaudeSonnet35Latest],
            ClaudeSonnet4Alias => [ClaudeSonnet4Alias, ClaudeSonnet37Latest, ClaudeSonnet35Latest],
            ClaudeSonnet37Latest => [ClaudeSonnet37Latest, ClaudeSonnet35Latest],
            ClaudeSonnet35Latest => [ClaudeSonnet35Latest],
            _ => [requestedModel, ClaudeSonnet37Latest, ClaudeSonnet35Latest]
        };
    }
}
