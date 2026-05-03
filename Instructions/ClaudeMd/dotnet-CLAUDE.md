# CLAUDE.md — .NET / C# project recipe

<!--
  Place this file at: CLAUDE.md (repo root)
  Claude Code reads it automatically when you open the repo.
  Claude.ai users can upload it for project context.
  Customize the sections marked [YOUR PROJECT] before committing.
-->

## Project

[YOUR PROJECT] — [brief one-line description].

Built with .NET 9, C# 13. See `README.md` for the full overview.

## Essential commands

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run the main project
dotnet run --project src/[YourProject]

# Format
dotnet format

# Add a package
dotnet add package [PackageName]
```

## Architecture

[Describe the high-level structure here — 3-5 bullet points max.]

- `src/[YourProject]/Orchestrators/` — multi-agent workflow coordination
- `src/[YourProject]/Skills/`        — reusable Markdown skill packs and references
- `src/[YourProject]/Prompts/`       — versioned prompt templates (`.txt`)
- `src/[YourProject].Tests/`         — xUnit unit tests

## Code conventions

- **Async**: always `async/await`, always propagate `CancellationToken`
- **HTTP**: inject `IHttpClientFactory`, never `new HttpClient()` in hot paths
- **DI**: register in `Program.cs`; prefer primary constructors for injection
- **Records**: use `record` for immutable DTOs
- **Logging**: `ILogger<T>` with structured message templates
- **Namespaces**: file-scoped always; no `#region`

## Key patterns in this repo

### Agent pattern
Every agent implements `IAgent`. The orchestrator discovers agents via DI,
calls `CanHandle(ctx)` to filter, then runs eligible agents concurrently
with `Task.WhenAll`.

### Prompt loading
Prompts live in `Prompts/*.txt` and are loaded at runtime via `IPromptLoader`.
Use `{{VARIABLE}}` for substitution slots. Never hardcode prompts in C# files.

### Synthesis
After agents run, the orchestrator calls Claude with all agent outputs
concatenated as labeled sections, asking for a unified synthesis.

## What to avoid

- ❌ `new HttpClient()` in methods — always inject or use factory
- ❌ `.Result` / `.Wait()` — always `await`
- ❌ Hardcoded API keys — use environment variables or `IConfiguration`
- ❌ Business logic in `Program.cs` beyond wiring
- ❌ Catching `Exception` at low levels

## Environment variables

| Variable | Required | Description |
|---|---|---|
| `ANTHROPIC_API_KEY` | ✅ | Anthropic API key |

## Testing

```bash
dotnet test --logger "console;verbosity=normal"
```

- Framework: xUnit + FluentAssertions + NSubstitute
- Naming: `{Class}Tests` / `{Method}_{Scenario}_{ExpectedResult}`
- Mock all HTTP and external I/O at the boundary
