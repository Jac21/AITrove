# Summarization Prompt Template

Use this as the base prompt structure for a one-shot summarization call.

```text
Summarize the following content in {{MAX_WORDS}} words or fewer.

Audience: {{AUDIENCE}}
Focus: {{FOCUS}}

Requirements:
- Preserve the core meaning and the most important facts.
- Keep concrete decisions, constraints, and risks when present.
- Remove repetition and filler.
- Return only the summary text.

Content:
{{TEXT}}
```

Notes:

- If `audience` is not provided, remove that line.
- If `focus` is not provided, remove that line.
- If `max_words` is not provided, replace the first line with:
  `Summarize the following content clearly and concisely.`
