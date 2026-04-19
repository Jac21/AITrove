# Skill authoring guide

A **skill** in AITrove is a stateless, single-purpose function that wraps one Claude API call.
Skills are the atoms of the trove — agents and orchestrators compose them.

## Anatomy of a skill

```
Skills/
└── YourSkill.cs      ← stateless static class or thin service
```

A skill:

- Takes plain inputs (strings, primitives, small DTOs)
- Makes exactly one Claude API call
- Returns a plain output (string, structured record)
- Carries no state between calls
- Is independently testable with a mocked HTTP client

## When to use a skill vs an agent

| Use a **skill** when…                     | Use an **agent** when…                   |
| ----------------------------------------- | ---------------------------------------- |
| The task is one API call                  | The task needs context from other agents |
| The function is reusable across workflows | The logic is specific to one workflow    |
| You want a utility others can import      | You need `CanHandle` routing logic       |

## Skill template (C#)

```csharp
namespace AITrove.Skills;

public static class YourSkill
{
    public static async Task<string> RunAsync(
        string input,
        string apiKey,
        CancellationToken ct = default)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("x-api-key", apiKey);
        http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var body = new
        {
            model                = "claude-haiku-4-5-20251001",   // use Haiku for cheap one-shots
            max_tokens_to_sample = 256,
            system               = "Answer succinctly and clearly.",
            messages             = new[]
            {
                new
                {
                    role    = "user",
                    content = new[] { new { type = "text", text = $"Your prompt here: {input}" } }
                }
            }
        };

        var resp = await http.PostAsJsonAsync("https://api.anthropic.com/v1/messages", body, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadFromJsonAsync<AnthropicResponse>(cancellationToken: ct);
        return json?.Content?[0].Text ?? string.Empty;
    }
}
```

## Model selection guide

| Use case                            | Recommended model           | Why          |
| ----------------------------------- | --------------------------- | ------------ |
| Quick classification, summarization | `claude-haiku-4-5-20251001` | Fast + cheap |
| Code review, analysis, reasoning    | `claude-sonnet-4-20250514`  | Balanced     |
| Complex multi-step reasoning        | `claude-opus-4-20250514`    | Most capable |

## Existing skills

| Skill                   | What it does                        | Model |
| ----------------------- | ----------------------------------- | ----- |
| `SummarizationSkill.cs` | Summarize arbitrary text to N words | Haiku |

## Adding a new skill

1. Create `YourSkillName.cs` in `Skills/`
2. Follow the template above
3. Add a row to the table above
4. Add a matching prompt to `Prompts/` if the prompt is non-trivial
