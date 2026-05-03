using AITrove;
using AITrove.Agents.Implementations;
using AITrove.Agents.Interfaces;
using AITrove.Context;
using AITrove.Prompts.Interfaces;
using AITrove.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

CliRequest? request;
try
{
    request = await CliRequestParser.ParseAsync(args, Console.In, Console.IsInputRedirected);
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    Console.Error.WriteLine();
    Console.Error.WriteLine(CliRequestParser.BuildUsage());
    return;
}

if (request is null)
{
    Console.WriteLine(CliRequestParser.BuildUsage());
    return;
}

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
    SystemPrompt = request.SystemPrompt,
    UserMessage  = request.UserMessage,
    Metadata     =
    {
        ["code"] = request.Code
    }
};

var result = await orchestrator.RunAsync(ctx);
Console.WriteLine("\n=== Synthesis ===");
Console.WriteLine(result);
