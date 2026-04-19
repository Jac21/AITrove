# CopilotInstructions

Recipes for `.github/copilot-instructions.md` — the file GitHub Copilot reads
automatically in VS Code and Visual Studio to understand your project.

## How it works

Copilot reads `.github/copilot-instructions.md` at the repo root and uses it
as persistent context for every suggestion in that workspace. No configuration
needed beyond placing the file.

## Recipes

| File | Stack | Use case |
|---|---|---|
| `dotnet-copilot-instructions.md` | .NET 9 / C# | General .NET project baseline |

## Tips for effective Copilot instructions

- Be specific about what to avoid — negative constraints are often more useful than positive ones
- Include your test framework and naming conventions — Copilot will mirror them
- List your DI and HTTP patterns explicitly — it eliminates the most common anti-patterns
- Keep the file under ~150 lines; Copilot has a context budget

## Adding a new recipe

1. Create `{stack}-copilot-instructions.md`
2. Add a row to the table above
3. Mark all project-specific placeholders with `[YOUR PROJECT]`
