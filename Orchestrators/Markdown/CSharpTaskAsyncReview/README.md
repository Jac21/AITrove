# C# Task and Async Review Pipeline

This is a Markdown-bound agent workflow example. It describes a two-phase C# code review pipeline without requiring a runtime implementation.

The example highlights context passing between subagents:

1. `TaskPitfallReviewer` performs a broad pass over Task-based programming risks.
2. `AsyncAwaitReviewer` receives the original code plus a compressed handoff from phase 1, then performs deeper async/await analysis.

The important constraint is that phase 2 does not receive the full phase 1 transcript. It receives a compact, structured handoff so the context window stays small and focused.

## Files

| File | Purpose |
| --- | --- |
| `workflow.md` | Top-level coordinator instructions |
| `agents/task-pitfall-reviewer.md` | Phase 1 subagent prompt |
| `agents/async-await-reviewer.md` | Phase 2 subagent prompt |
| `references/compressed-handoff-contract.md` | Shared context-passing contract |
| `examples/sample-invocation.md` | Example input and expected workflow shape |

## Intended Use

Use this as a prompt design reference for:

- Markdown-only agent demonstrations
- multi-phase code review workflows
- context compression patterns
- handoff contracts between specialized subagents

This example is deliberately light. It is meant to demonstrate orchestration shape, not replace a full static analyzer or production code review system.
