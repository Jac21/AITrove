# CursorRules

Recipes for Cursor's AI instruction files — `.cursor/rules/*.mdc` (modern)
or `.cursorrules` (legacy).

## How it works

**Modern Cursor (0.40+)**: Place `.mdc` files in `.cursor/rules/` at your repo root.
Each file has MDC frontmatter that controls when it activates:

```
---
description: What this rule does
globs: ["**/*.cs"]       # only activate for .cs files
alwaysApply: false       # true = always in context, false = on-demand
---
```

Multiple rule files can coexist — Cursor loads the relevant ones per file type.

**Legacy Cursor**: Place `.cursorrules` at the repo root. Plain markdown, always active.
No scoping support.

## Recipes

| File | Stack | Globs | Always active? |
|---|---|---|---|
| `dotnet-cursor-rules.mdc` | .NET 9 / C# | `**/*.cs`, `**/*.csproj` | No (C# files only) |

## Tips for effective Cursor rules

- Use `globs` to scope rules to file types — keeps context budget lean
- The "what to never suggest" section has outsized impact; Cursor's default suggestions
  often include exactly the anti-patterns you want to ban
- List your naming conventions explicitly — Cursor will mirror them in generated code
- Keep each rule file focused on one concern; compose them rather than making one giant file

## MDC frontmatter reference

```
---
description: Short description shown in Cursor's rules UI
globs: ["**/*.ts", "src/**/*.tsx"]
alwaysApply: true
---
```

`alwaysApply: true` — rule is always injected into context regardless of current file.
Use sparingly; it consumes context budget for every interaction.

## Adding a new recipe

1. Create `{stack}-cursor-rules.mdc`
2. Set appropriate `globs` in the frontmatter
3. Add a row to the table above
4. Mark all project-specific placeholders with `[YOUR PROJECT]`
