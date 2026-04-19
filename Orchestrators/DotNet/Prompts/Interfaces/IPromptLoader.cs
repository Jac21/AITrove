namespace AITrove.Prompts.Interfaces;

/// <summary>
/// Loads named prompt templates from disk (or any backing store).
/// Inject FilePromptLoader in production; inject a mock in tests.
/// </summary>
public interface IPromptLoader
{
    /// <summary>
    /// Load a prompt template by name (without extension).
    /// E.g. "code-review.system" resolves to Prompts/code-review.system.txt
    /// </summary>
    Task<string> LoadAsync(string name, CancellationToken ct = default);
}

/// <summary>Default implementation — loads .txt files from the Prompts/ directory.</summary>
public sealed class FilePromptLoader(string promptsDirectory) : IPromptLoader
{
    public async Task<string> LoadAsync(string name, CancellationToken ct = default)
    {
        var path = Path.Combine(promptsDirectory, $"{name}.md");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Prompt template not found: {path}");

        return await File.ReadAllTextAsync(path, ct);
    }
}
