namespace AITrove.Context;

/// <summary>
/// Immutable per-run context. Agents read from it; only the orchestrator writes results into it.
/// </summary>
public sealed class AgentContext
{
    public required string ApiKey       { get; init; }
    public required string UserMessage  { get; init; }
    public required string SystemPrompt { get; init; }

    /// <summary>
    /// Freeform metadata agents can query.
    /// Convention: use lowercase snake_case keys, e.g. "code", "language", "pr_diff".
    /// </summary>
    public Dictionary<string, object> Metadata { get; } = [];

    /// <summary>Accumulated outputs from agents that have already run in this workflow.</summary>
    private readonly List<(string Name, string Output)> _results = [];

    public void AddAgentResult(string name, string output) =>
        _results.Add((name, output));

    public IReadOnlyList<(string Name, string Output)> AgentResults =>
        _results.AsReadOnly();

    /// <summary>
    /// Builds the synthesis prompt by prepending all agent outputs as labeled
    /// sections before the original user message. Fed to Claude for final synthesis.
    /// </summary>
    public string BuildSynthesisPrompt()
    {
        var sb = new System.Text.StringBuilder();

        foreach (var (name, output) in _results)
        {
            sb.AppendLine($"### {name} findings");
            sb.AppendLine(output);
            sb.AppendLine();
        }

        sb.AppendLine("### User request");
        sb.Append(UserMessage);
        return sb.ToString();
    }
}
