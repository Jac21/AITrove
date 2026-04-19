# ClaudeMd

Recipes for `CLAUDE.md` — the file Claude Code reads automatically at repo root,
and that you can upload to claude.ai for persistent project context.

## How it works

**Claude Code**: Place `CLAUDE.md` at the root of your repo. Claude Code reads it
on startup and uses it as grounding context for every interaction in that workspace.
Sub-directories can have their own `CLAUDE.md` for scoped context.

**Claude.ai**: Upload `CLAUDE.md` as a project document. It will be included
in the context of every conversation in that project.

## Recipes

| File | Stack | Use case |
|---|---|---|
| `dotnet-CLAUDE.md` | .NET 9 / C# | General .NET project baseline |

## Anatomy of a great CLAUDE.md

A `CLAUDE.md` should answer the questions Claude would otherwise have to ask:

1. **What is this project?** — one sentence
2. **How do I run it?** — essential commands only
3. **How is it structured?** — key directories and what lives there
4. **How should code be written?** — conventions, patterns, anti-patterns
5. **What environment does it need?** — env vars, dependencies

Keep it under ~200 lines. Claude has a large context window but a focused
`CLAUDE.md` produces more consistent results than a sprawling one.

## Tips

- Use `## Essential commands` as the first technical section — Claude reaches for it immediately
- Include your anti-pattern list (`## What to avoid`) — it's the highest-leverage section
- List environment variables in a table — Claude will reference it when debugging missing config
- For monorepos, add a root `CLAUDE.md` for the whole repo and per-package `CLAUDE.md` files for specifics

## Adding a new recipe

1. Create `{stack}-CLAUDE.md`
2. Add a row to the table above
3. Mark all project-specific placeholders with `[YOUR PROJECT]`
