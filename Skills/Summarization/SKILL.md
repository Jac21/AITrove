# Summarization

Summarize arbitrary text into a concise output that preserves the most important meaning, constraints, and decisions.

## When to use this skill

Use this skill when you need to:

- compress long notes, docs, code reviews, transcripts, or diffs
- produce a quick executive summary before deeper analysis
- extract the highest-signal takeaways for another agent, workflow, or human

Do not use this skill when the task primarily requires:

- factual verification against external sources
- line-by-line code review with bug finding
- rewriting content into a different tone or format without summarization

## Inputs

- `text`: the source material to summarize
- `max_words`:
  recommended default is `120`
- `audience`:
  optional; examples include `engineers`, `executives`, `support`, `general`
- `focus`:
  optional; examples include `risks`, `decisions`, `customer impact`, `action items`

## Output

Return only the summary text unless the caller explicitly asks for sections or bullets.

## Instructions

1. Read the full input before summarizing.
2. Preserve concrete facts, decisions, constraints, and risks.
3. Remove repetition, hedging, filler, and low-signal detail.
4. Prefer specific nouns and verbs over vague abstractions.
5. Stay within `max_words` when one is provided.
6. If the input is ambiguous or incomplete, summarize only what is present and do not invent missing context.
7. If `focus` is provided, bias the summary toward that lens while still preserving the main point of the source.
8. If `audience` is provided, tune density and terminology accordingly.

## Prompt Pattern

Use the template in [references/prompt-template.md](references/prompt-template.md) as the base prompt shape.

Apply the quality checks in [references/quality-checklist.md](references/quality-checklist.md) before returning the final answer.

## Example Invocation

Input:

- `text`: a long design review thread
- `max_words`: `100`
- `audience`: `engineers`
- `focus`: `decisions and follow-up work`

Expected behavior:

- produce one concise summary under 100 words
- preserve the main decision, open issues, and next steps
- omit greetings, repetition, and side discussion
