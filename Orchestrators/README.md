# Orchestrators

Coordination patterns that fan work out to multiple agents and synthesize a unified result.

## Pattern: fan-out + synthesize

```
AgentContext
    │
    ├──▶ Agent A (CanHandle? ✅) ──▶ output A ──┐
    ├──▶ Agent B (CanHandle? ✅) ──▶ output B ──┼──▶ Claude synthesis ──▶ final answer
    └──▶ Agent C (CanHandle? ❌) skipped         │
                                                 └── (all outputs labeled in prompt)
```

Each agent runs independently and concurrently. The orchestrator collects outputs and
asks Claude to synthesize a final answer from all agent findings plus the original user message.

## Runtimes

| Folder | Runtime | Status |
|--------|---------|--------|
| `DotNet/` | .NET 9, C# | ✅ Reference implementation |
| `Python/` | Python 3.12+, asyncio | ✅ RAG-oriented boilerplate |
| `TypeScript/` | Node 22, Promise.all | 🔜 Planned |

## Adding a new agent (.NET)

1. Create `YourAgent.cs` implementing `IAgent`
2. Implement `CanHandle` to gate on a `ctx.Metadata` key
3. Register in `Program.cs`: `.AddTransient<IAgent, YourAgent>()`
4. Add corresponding prompt files to `Prompts/` if needed

## Adding a new agent (Python)

1. Create a new agent in `Python/aitrove_python/agents/`
2. Implement `name`, `can_handle(ctx)`, and `async execute(ctx)`
3. Add prompt files under `Python/prompts/` when needed
4. Register the agent in `Python/aitrove_python/__main__.py`
