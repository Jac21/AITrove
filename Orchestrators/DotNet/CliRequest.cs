namespace AITrove;

public sealed record CliRequest(
    string Code,
    string UserMessage,
    string SystemPrompt);

public static class CliRequestParser
{
    public const string DefaultUserMessage =
        "Please review my code and summarize the findings.";

    public const string DefaultSystemPrompt =
        "You are a senior .NET engineer. Synthesize the agent findings below into a concise, prioritized summary.";

    public static async Task<CliRequest?> ParseAsync(
        string[] args,
        TextReader stdin,
        bool isInputRedirected,
        CancellationToken ct = default)
    {
        string? inlineCode = null;
        string? codeFile = null;
        var readFromStdin = false;
        var userMessage = DefaultUserMessage;
        var systemPrompt = DefaultSystemPrompt;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--help":
                case "-h":
                    return null;

                case "--code":
                    inlineCode = ReadRequiredValue(args, ref i, "--code");
                    break;

                case "--code-file":
                    codeFile = ReadRequiredValue(args, ref i, "--code-file");
                    break;

                case "--stdin":
                    readFromStdin = true;
                    break;

                case "--prompt":
                    userMessage = ReadRequiredValue(args, ref i, "--prompt");
                    break;

                case "--system-prompt":
                    systemPrompt = ReadRequiredValue(args, ref i, "--system-prompt");
                    break;

                default:
                    throw new ArgumentException($"Unknown argument: {args[i]}");
            }
        }

        if (inlineCode is not null && codeFile is not null)
            throw new ArgumentException("Choose only one of --code or --code-file.");

        var explicitSources = (inlineCode is not null ? 1 : 0) + (codeFile is not null ? 1 : 0) + (readFromStdin ? 1 : 0);
        if (explicitSources > 1)
            throw new ArgumentException("Choose only one code input source: --code, --code-file, or --stdin.");

        if (inlineCode is not null)
            return new CliRequest(inlineCode, userMessage, systemPrompt);

        if (codeFile is not null)
        {
            var code = await File.ReadAllTextAsync(codeFile, ct);
            return new CliRequest(code, userMessage, systemPrompt);
        }

        if (readFromStdin || isInputRedirected)
        {
            var code = await stdin.ReadToEndAsync(ct);
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("No code was provided on stdin.");

            return new CliRequest(code, userMessage, systemPrompt);
        }

        throw new ArgumentException(
            "No code input was provided. Use --code, --code-file, or pipe content on stdin.");
    }

    public static string BuildUsage() =>
        """
        Usage:
          dotnet run --project Orchestrators/DotNet -- --code-file path/to/file.cs
          dotnet run --project Orchestrators/DotNet -- --code "public class Example { }"
          cat path/to/file.cs | dotnet run --project Orchestrators/DotNet -- --stdin

        Options:
          --code <text>             Review inline code passed on the command line
          --code-file <path>        Review code loaded from a file
          --stdin                   Read code from standard input
          --prompt <text>           Override the synthesis request shown to the orchestrator
          --system-prompt <text>    Override the system prompt used for synthesis
          --help, -h                Show this help text
        """;

    private static string ReadRequiredValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
            throw new ArgumentException($"Missing value for {optionName}.");

        index++;
        return args[index];
    }
}
