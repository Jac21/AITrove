# Skills

This folder contains cross-platform skill definitions in standard Markdown format.

Skills are reusable instruction packs: small, focused capabilities that can be invoked from different runtimes, agents, or orchestrators without depending on a specific implementation language.

## Folder layout

```text
Skills/
├── SKILL.md
└── YourSkill/
    ├── SKILL.md
    └── references/
        └── supporting-file.md
```

## What belongs in a skill

A skill should:

- solve one focused problem well
- be portable across platforms and runtimes
- describe inputs, outputs, constraints, and quality bar
- link to reference files instead of embedding too much repeated detail

A skill should not:

- assume a specific SDK, framework, or programming language unless the skill is intentionally platform-specific
- duplicate large reference material directly in the main `SKILL.md`
- carry runtime state between invocations

## When to use a skill vs an agent

| Use a **skill** when… | Use an **agent** when… |
| --- | --- |
| The task is narrow and repeatable | The task needs routing, multi-step execution, or orchestration |
| The instructions should be reusable across platforms | The logic is tied to one workflow or runtime |
| You want a durable prompt/instruction asset | You need tool coordination and context accumulation |

## Skill authoring template

Each skill folder should contain a `SKILL.md` that covers:

1. What the skill does
2. When to use it
3. Expected inputs
4. Expected output shape
5. Step-by-step instructions
6. Links to any `references/` files

## Existing skills

| Skill folder | What it does |
| --- | --- |
| `Summarization/` | Summarize arbitrary text into a concise, audience-aware summary |

## Adding a new skill

1. Create `Skills/YourSkill/`
2. Add `Skills/YourSkill/SKILL.md`
3. Add any reusable supporting material under `Skills/YourSkill/references/`
4. Add a row to the table above
