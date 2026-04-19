using AITrove;
using AITrove.Agents.Implementations;
using AITrove.Agents.Interfaces;
using AITrove.Context;
using AITrove.Prompts.Interfaces;
using AITrove.Workflows;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AITrove.Orchestrators.DotNet.Tests;

public sealed class AnthropicWorkflowTests
{
    [Fact]
    public async Task CodeReviewAgent_UsesTypedAnthropicClientThroughInjection()
    {
        var prompts = new FakePromptLoader(new Dictionary<string, string>
        {
            ["code-review.system"] = "Review this C# code carefully.",
            ["code-review.user"] = "Please review:\n{{CODE}}"
        });
        var anthropic = new RecordingAnthropicClient("review-result");
        var agent = new CodeReviewAgent(prompts, anthropic);
        var ctx = new AgentContext
        {
            ApiKey = "test-key",
            SystemPrompt = "unused",
            UserMessage = "unused"
        };
        ctx.Metadata["code"] = "Console.WriteLine(\"hi\");";

        var result = await agent.ExecuteAsync(ctx);

        Assert.Equal("review-result", result);
        var call = Assert.Single(anthropic.Calls);
        Assert.Equal("test-key", call.ApiKey);
        Assert.Equal(AnthropicModelIds.ClaudeSonnet4, call.Model);
        Assert.Equal(512, call.MaxTokens);
        Assert.Equal("Review this C# code carefully.", call.SystemPrompt);
        Assert.Contains("Console.WriteLine(\"hi\");", call.UserMessage);
    }

    [Fact]
    public async Task WorkflowOrchestrator_SynthesizesOnlySuccessfulAgentOutputs()
    {
        var anthropic = new RecordingAnthropicClient("final-summary");
        var agents = new IAgent[]
        {
            new StubAgent("SuccessAgent", canHandle: true, output: "Use a shared HttpClient."),
            new StubAgent("FailingAgent", canHandle: true, output: "unused", exception: new InvalidOperationException("boom")),
            new StubAgent("IgnoredAgent", canHandle: false, output: "should never run")
        };
        var orchestrator = new WorkflowOrchestrator(
            agents,
            anthropic,
            NullLogger<WorkflowOrchestrator>.Instance);
        var ctx = new AgentContext
        {
            ApiKey = "test-key",
            SystemPrompt = "Synthesize findings.",
            UserMessage = "Review this diff."
        };

        var result = await orchestrator.RunAsync(ctx);

        Assert.Equal("final-summary", result);
        var call = Assert.Single(anthropic.Calls);
        Assert.Equal(1024, call.MaxTokens);
        Assert.Equal("Synthesize findings.", call.SystemPrompt);
        Assert.Contains("### SuccessAgent findings", call.UserMessage);
        Assert.Contains("Use a shared HttpClient.", call.UserMessage);
        Assert.DoesNotContain("### FailingAgent findings", call.UserMessage);
        Assert.Contains("### User request", call.UserMessage);
        Assert.Contains("Review this diff.", call.UserMessage);
    }

    [Fact]
    public async Task WorkflowOrchestrator_WithNoEligibleAgents_SynthesizesOriginalRequest()
    {
        var anthropic = new RecordingAnthropicClient("direct-summary");
        var orchestrator = new WorkflowOrchestrator(
            [new StubAgent("IgnoredAgent", canHandle: false, output: "unused")],
            anthropic,
            NullLogger<WorkflowOrchestrator>.Instance);
        var ctx = new AgentContext
        {
            ApiKey = "test-key",
            SystemPrompt = "Synthesize findings.",
            UserMessage = "Please review my code."
        };

        var result = await orchestrator.RunAsync(ctx);

        Assert.Equal("direct-summary", result);
        var call = Assert.Single(anthropic.Calls);
        Assert.DoesNotContain("findings", call.UserMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("### User request", call.UserMessage);
        Assert.Contains("Please review my code.", call.UserMessage);
    }

    [Fact]
    public void AnthropicModelIds_ResolvesFallbackChainForClaude4()
    {
        Environment.SetEnvironmentVariable(AnthropicModelIds.ModelOverrideEnvironmentVariable, null);

        var candidates = AnthropicModelIds.ResolveModelCandidates(AnthropicModelIds.ClaudeSonnet4);

        Assert.Equal(
            [
                AnthropicModelIds.ClaudeSonnet4,
                AnthropicModelIds.ClaudeSonnet4Alias,
                AnthropicModelIds.ClaudeSonnet37Latest,
                AnthropicModelIds.ClaudeSonnet35Latest
            ],
            candidates);
    }

    [Fact]
    public void AnthropicModelIds_UsesEnvironmentOverrideWhenProvided()
    {
        const string overrideModel = "claude-sonnet-4-6";
        Environment.SetEnvironmentVariable(AnthropicModelIds.ModelOverrideEnvironmentVariable, overrideModel);
        try
        {
            var candidates = AnthropicModelIds.ResolveModelCandidates(AnthropicModelIds.ClaudeSonnet4);

            Assert.Equal([overrideModel], candidates);
        }
        finally
        {
            Environment.SetEnvironmentVariable(AnthropicModelIds.ModelOverrideEnvironmentVariable, null);
        }
    }

    private sealed class FakePromptLoader(Dictionary<string, string> prompts) : IPromptLoader
    {
        public Task<string> LoadAsync(string name, CancellationToken ct = default) =>
            Task.FromResult(prompts[name]);
    }

    private sealed class RecordingAnthropicClient(string response) : IAnthropicMessageClient
    {
        public List<AnthropicCall> Calls { get; } = [];

        public Task<string> SendMessageAsync(
            string apiKey,
            string model,
            int maxTokens,
            string systemPrompt,
            string userMessage,
            CancellationToken ct = default)
        {
            Calls.Add(new AnthropicCall(apiKey, model, maxTokens, systemPrompt, userMessage));
            return Task.FromResult(response);
        }
    }

    private sealed class StubAgent(
        string name,
        bool canHandle,
        string output,
        Exception? exception = null) : IAgent
    {
        public string Name => name;

        public bool CanHandle(AgentContext ctx) => canHandle;

        public Task<string> ExecuteAsync(AgentContext ctx, CancellationToken ct = default) =>
            exception is null ? Task.FromResult(output) : Task.FromException<string>(exception);
    }

    private sealed record AnthropicCall(
        string ApiKey,
        string Model,
        int MaxTokens,
        string SystemPrompt,
        string UserMessage);
}
