using AITrove;
using AITrove.Agents.Implementations;
using AITrove.Agents.Interfaces;
using AITrove.Context;
using AITrove.Prompts.Interfaces;
using AITrove.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection()
    .AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Debug))
    .AddSingleton<IPromptLoader>(_ =>
        new FilePromptLoader(Path.Combine(AppContext.BaseDirectory, "Prompts")))
    .AddSingleton<IAnthropicMessageClient, AnthropicMessageClient>()
    .AddTransient<IAgent, CodeReviewAgent>()
    // Register additional IAgent implementations here as the trove grows:
    // .AddTransient<IAgent, ArchitectureReviewAgent>()
    // .AddTransient<IAgent, TestCoverageAgent>()
    .AddTransient<WorkflowOrchestrator>()
    .BuildServiceProvider();

var orchestrator = services.GetRequiredService<WorkflowOrchestrator>();

var ctx = new AgentContext
{
    ApiKey       = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? throw new InvalidOperationException("ANTHROPIC_API_KEY not set"),
    SystemPrompt = "You are a senior .NET engineer. Synthesize the agent findings below into a concise, prioritized summary.",
    UserMessage  = "Please review my code and summarize the findings.",
    Metadata     =
    {
        ["code"] = """
            public async Task<string> GetDataAsync(string url)
            {
                var client = new HttpClient();
                var response = await client.GetAsync(url);
                return await response.Content.ReadAsStringAsync();
            }
            """
    }
};

var result = await orchestrator.RunAsync(ctx);
Console.WriteLine("\n=== Synthesis ===");
Console.WriteLine(result);
