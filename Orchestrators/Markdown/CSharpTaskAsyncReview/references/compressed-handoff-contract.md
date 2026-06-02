# Compressed Handoff Contract

This contract keeps intermediate agent output small while preserving the context needed by the next subagent.

## Artifact Name

`TaskReviewHandoff`

## Format

Use this Markdown block exactly:

```markdown
## TaskReviewHandoff

### Scope
- reviewed_area:
- assumptions:
- omitted:

### Risk Map
| id | severity | pattern | symbol_or_location | confidence | why_it_matters |
| --- | --- | --- | --- | --- | --- |

### Priority Symbols
- symbol:
  reason:

### Phase 2 Questions
- question:
  linked_risk_ids:

### Compression Notes
- code_quoted_lines:
- dropped_context:
```

## Field Rules

`reviewed_area`:

- one sentence describing what was reviewed

`assumptions`:

- only assumptions that affect phase 2

`omitted`:

- mention important content that was intentionally not reviewed

`Risk Map`:

- maximum 8 rows
- `severity` must be `high`, `medium`, or `low`
- `pattern` should be short, such as `sync-over-async` or `unbounded fan-out`
- `confidence` must be `high`, `medium`, or `low`
- `why_it_matters` must be one concise sentence

`Priority Symbols`:

- maximum 6 entries
- use method, type, file, or call-site names when visible

`Phase 2 Questions`:

- maximum 6 questions
- questions must be directly useful to the async/await reviewer

`Compression Notes`:

- `code_quoted_lines` must be a number
- `dropped_context` should summarize what was intentionally excluded to save tokens

## Token Discipline

The handoff should carry decision-relevant context only.

Good handoff content:

- `GetDataAsync`: uses `new HttpClient()` and lacks cancellation
- `FetchAllAsync`: potential unbounded `Task.WhenAll`
- question for phase 2: verify exception propagation from fan-out

Poor handoff content:

- long explanation of how `Task` works
- complete code listings
- speculative rewrites without evidence
- repeated observations that phase 2 can infer from the code
