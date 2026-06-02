# Workflow: C# Task and Async Review

## Goal

Review a C# solution or code excerpt for Task-based programming and async/await pitfalls using two sequential subagents.

The workflow must demonstrate compressed context transfer from phase 1 to phase 2.

## Inputs

- `code`: C# source, diff, or selected solution excerpt
- `review_scope`: optional caller guidance such as `focus on service layer` or `review public APIs only`
- `risk_tolerance`: optional severity threshold such as `only actionable issues`

## Subagents

### Phase 1: TaskPitfallReviewer

Prompt:

- [agents/task-pitfall-reviewer.md](agents/task-pitfall-reviewer.md)

Responsibilities:

- identify broad Task-based programming risks
- avoid deep async/await explanation
- emit only the compressed handoff contract

Output:

- `TaskReviewHandoff`

### Phase 2: AsyncAwaitReviewer

Prompt:

- [agents/async-await-reviewer.md](agents/async-await-reviewer.md)

Inputs:

- original `code`
- compressed `TaskReviewHandoff` from phase 1

Responsibilities:

- perform detailed async/await review
- use phase 1 context to prioritize areas of concern
- produce final findings for the caller

## Handoff Contract

Both subagents must follow:

- [references/compressed-handoff-contract.md](references/compressed-handoff-contract.md)

## Coordinator Instructions

1. Invoke `TaskPitfallReviewer` with the original code and caller scope.
2. Require `TaskPitfallReviewer` to return only `TaskReviewHandoff`.
3. Validate that the handoff is compact and does not include full raw notes.
4. Invoke `AsyncAwaitReviewer` with:
   - original code
   - `TaskReviewHandoff`
   - caller scope
5. Return only the phase 2 final review.

## Context Compression Rule

Phase 1 output should be treated as an intermediate artifact, not a user-facing answer.

The coordinator must not pass these phase 1 materials to phase 2:

- full chain-of-thought style reasoning
- broad explanatory prose
- duplicated code snippets longer than 12 lines total
- low-confidence observations that do not affect phase 2 prioritization

The coordinator should pass only:

- top risk areas
- relevant symbols or files
- suspected patterns
- confidence and severity
- review instructions for phase 2

## Final Output Shape

The final response should contain:

1. `Summary`
2. `Findings`
3. `Recommended Fixes`
4. `Residual Risk`

Keep findings actionable. If no issue is found, state that explicitly and identify what was checked.
