# DotNet Orchestrator

This project contains the .NET orchestration sample for AITrove's agent workflow.

## Prerequisites

- .NET SDK 9 or newer
- `ANTHROPIC_API_KEY` for live Anthropic calls

## Live Run

From the repository root:

```bash
ANTHROPIC_API_KEY=your-key-here dotnet run --project Orchestrators/DotNet -- --code-file path/to/file.cs
```

From the project folder:

```bash
cd Orchestrators/DotNet
ANTHROPIC_API_KEY=your-key-here dotnet run -- --code-file path/to/file.cs
```

The CLI now accepts real code input instead of the old hardcoded sample. You can provide content three ways:

- `--code-file path/to/file.cs`
- `--code "public class Example { }"`
- `--stdin` or a redirected pipe

Examples:

```bash
ANTHROPIC_API_KEY=your-key-here dotnet run --project Orchestrators/DotNet -- --code-file src/MyService.cs
```

```bash
git diff -- MyFile.cs | ANTHROPIC_API_KEY=your-key-here dotnet run --project Orchestrators/DotNet -- --stdin --prompt "Review this diff and summarize the highest-risk findings."
```

```bash
ANTHROPIC_API_KEY=your-key-here dotnet run --project Orchestrators/DotNet -- --code "public async Task<string> GetAsync(HttpClient http) => await http.GetStringAsync(\"/\");"
```

Optional prompt controls:

- `--prompt "Focus on correctness and security risks."`
- `--system-prompt "You are a staff .NET reviewer. Return a concise severity-ranked summary."`

### Model Selection

By default, the orchestrator now tries these Anthropic API model names in order:

- `claude-sonnet-4-20250514`
- `claude-sonnet-4-0`
- `claude-sonnet-4-6`
- `claude-3-5-sonnet-latest`

This helps when an account does not yet have access to the Claude 4 snapshot even though the model is documented by Anthropic.

To force a specific model for live runs, set `AITROVE_ANTHROPIC_MODEL`:

```bash
AITROVE_ANTHROPIC_MODEL=claude-sonnet-4-6 ANTHROPIC_API_KEY=your-key-here dotnet run --project Orchestrators/DotNet
```

## Integration Tests

The test project lives at `Orchestrators/DotNet.Tests` and exercises the orchestration flow without making live Anthropic requests.

Run the suite from the repository root:

```bash
dotnet test Orchestrators/DotNet.Tests/AITrove.Orchestrators.DotNet.Tests.csproj
```

What these tests cover:

- `CodeReviewAgent` builds the expected Anthropic request through the injected `IAnthropicMessageClient`
- `WorkflowOrchestrator` only includes successful agent outputs in the synthesis prompt
- `WorkflowOrchestrator` falls back to direct synthesis when no agents are eligible

These are integration-style tests around the .NET orchestration layer: they use the real agent/orchestrator classes and replace only the outbound Anthropic transport with a fake implementation.

## Live Validation vs. Test Validation

`dotnet test` validates request construction, prompt flow, and orchestration behavior without requiring network access or an API key.

`dotnet run` is the live smoke test. Use it when you want to verify:

- the configured Anthropic API key is valid
- the selected model ID is available to your account
- the end-to-end request/response path succeeds against the live Anthropic service
