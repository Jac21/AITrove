# Instructions

Tooling-specific AI instruction files. Each subfolder contains recipes
for a different AI coding assistant — same intent, different file format and placement.

## Tooling map

| Folder | Tool | File location in your project | Format |
|---|---|---|---|
| `CopilotInstructions/` | GitHub Copilot | `.github/copilot-instructions.md` | Markdown |
| `ClaudeMd/` | Claude (claude.ai / Claude Code) | `CLAUDE.md` at repo root | Markdown |
| `CursorRules/` | Cursor | `.cursor/rules/*.mdc` or `.cursorrules` | Markdown / MDC |

## Philosophy

These files tell the AI assistant:
1. **What** the project is
2. **How** code should be written (conventions, patterns, stack)
3. **What** to avoid or always do
4. **Where** things live

A good instruction file is concise, specific, and opinionated.
Vague instructions produce vague assistance.

## Usage

Copy the relevant recipe into your project at the path shown above,
then customize the project-specific sections.
