using AITrove.Agents.Interfaces;
using AITrove.Context;
using Microsoft.Extensions.Logging;

namespace AITrove.Workflows;

/// <summary>
/// Fan-out + synthesize orchestrator.
///
/// Pattern:
///   1. Discover eligible agents via IAgent.CanHandle(ctx)
///   2. Run them all concurrently with Task.WhenAll
///   3. Collect successful outputs into AgentContext
///   4. Ask Claude to synthesize a final answer
///
/// Failures in individual agents are logged and skipped — they never
/// block the workflow unless ALL agents fail.
/// </summary>
public sealed class WorkflowOrchestrator(
    IEnumerable<IAgent> agents,
    IAnthropicMessageClient anthropic,
    ILogger<WorkflowOrchestrator> logger)
{
    private readonly IReadOnlyList<IAgent> _agents = [.. agents];

    public async Task<string> RunAsync(AgentContext ctx, CancellationToken ct = default)
    {
        var eligible = _agents.Where(a => a.CanHandle(ctx)).ToList();
        logger.LogInformation("Workflow starting. Eligible agents: {Count}", eligible.Count);

        if (eligible.Count == 0)
        {
            logger.LogWarning("No eligible agents found. Passing user message directly to Claude.");
            return await SynthesizeAsync(ctx, ct);
        }

        // Fan out — all eligible agents run concurrently
        var results = await Task.WhenAll(eligible.Select(a => RunAgentSafe(a, ctx, ct)));

        // Collect successful outputs
        foreach (var r in results.Where(r => r.Success))
            ctx.AddAgentResult(r.AgentName, r.Output);

        logger.LogInformation(
            "{Success}/{Total} agents succeeded",
            results.Count(r => r.Success), results.Length);

        return await SynthesizeAsync(ctx, ct);
    }

    // -------------------------------------------------------------------------

    private async Task<AgentResult> RunAgentSafe(
        IAgent agent, AgentContext ctx, CancellationToken ct)
    {
        try
        {
            logger.LogDebug("Running agent: {Name}", agent.Name);
            var output = await agent.ExecuteAsync(ctx, ct);
            return new AgentResult(agent.Name, output, true);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Agent {Name} failed — skipping", agent.Name);
            return new AgentResult(agent.Name, string.Empty, false);
        }
    }

    private async Task<string> SynthesizeAsync(AgentContext ctx, CancellationToken ct)
    {
        return await anthropic.SendMessageAsync(
            ctx.ApiKey,
            AnthropicModelIds.ClaudeSonnet4,
            maxTokens: 1024,
            systemPrompt: ctx.SystemPrompt,
            userMessage: ctx.BuildSynthesisPrompt(),
            ct);
    }
}

// ---------------------------------------------------------------------------
public record AgentResult(string AgentName, string Output, bool Success);
