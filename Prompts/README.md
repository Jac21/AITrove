# Prompts

Versioned prompt templates, decoupled from code so they can be iterated independently.

## Conventions

- File naming: `{use-case}.{role}.txt` where role is `system` or `user`
- Use `{{VARIABLE}}` for runtime substitution slots
- System prompts define persona + constraints; user prompts are the actual task
- Keep system prompts under ~500 tokens; user prompts under ~200 tokens (plus injected content)

## Prompt inventory

| File | Role | Used by | Variables |
|---|---|---|---|
| `code-review.system.txt` | System | `CodeReviewAgent` | — |
| `code-review.user.txt`   | User   | `CodeReviewAgent` | `{{CODE}}` |

## Adding a new prompt

1. Create `{use-case}.system.txt` and/or `{use-case}.user.txt`
2. Document variables in the table above
3. Reference by name (without extension) in your agent via `IPromptLoader.LoadAsync("use-case.system")`
