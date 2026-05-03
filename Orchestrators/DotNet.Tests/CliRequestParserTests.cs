using AITrove;
using Xunit;

namespace AITrove.Orchestrators.DotNet.Tests;

public sealed class CliRequestParserTests
{
    [Fact]
    public async Task ParseAsync_UsesInlineCodeWhenProvided()
    {
        var request = await CliRequestParser.ParseAsync(
            ["--code", "public class Example { }", "--prompt", "Focus on bugs."],
            new StringReader(string.Empty),
            isInputRedirected: false);

        Assert.NotNull(request);
        Assert.Equal("public class Example { }", request.Code);
        Assert.Equal("Focus on bugs.", request.UserMessage);
        Assert.Equal(CliRequestParser.DefaultSystemPrompt, request.SystemPrompt);
    }

    [Fact]
    public async Task ParseAsync_ReadsRedirectedStdinWhenNoExplicitSourceIsProvided()
    {
        var request = await CliRequestParser.ParseAsync(
            [],
            new StringReader("public class Example { }\n"),
            isInputRedirected: true);

        Assert.NotNull(request);
        Assert.Contains("public class Example", request.Code);
    }

    [Fact]
    public async Task ParseAsync_ReadsCodeFromFile()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "public class Example { }\n");

            var request = await CliRequestParser.ParseAsync(
                ["--code-file", path, "--system-prompt", "Custom system prompt"],
                new StringReader(string.Empty),
                isInputRedirected: false);

            Assert.NotNull(request);
            Assert.Contains("public class Example", request.Code);
            Assert.Equal("Custom system prompt", request.SystemPrompt);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ParseAsync_ReturnsNullForHelp()
    {
        var request = await CliRequestParser.ParseAsync(
            ["--help"],
            new StringReader(string.Empty),
            isInputRedirected: false);

        Assert.Null(request);
    }
}
