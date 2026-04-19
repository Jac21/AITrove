using AITrove.Context;

namespace AITrove.Agents.Interfaces;

/// <summary>
/// Every agent in the trove implements this interface.
/// The orchestrator discovers agents via DI and routes work to them.
/// </summary>
public interface IAgent
{
    /// <summary>Human-readable agent name used in logging and synthesis context.</summary>
    string Name { get; }

    /// <summary>
    /// Return true if this agent is relevant to the given context.
    /// Returning false lets the orchestrator skip agents that don't apply
    /// without them needing to know about each other.
    /// </summary>
    bool CanHandle(AgentContext ctx);

    /// <summary>
    /// Execute the agent against the context.
    /// Throw on unrecoverable failure; the orchestrator will catch,
    /// log, and continue with remaining eligible agents.
    /// </summary>
    Task<string> ExecuteAsync(AgentContext ctx, CancellationToken ct = default);
}
