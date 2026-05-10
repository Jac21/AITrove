# Python RAG Orchestrator

This runtime mirrors the fan-out/synthesize pattern from the .NET implementation, but it is oriented toward retrieval-augmented generation system design instead of code review.

## What it does

The orchestrator fans a RAG brief out to specialized agents:

- `RagArchitectureAgent`:
  proposes the end-to-end RAG architecture and component boundaries
- `RetrievalStrategyAgent`:
  recommends chunking, indexing, retrieval, reranking, and grounding strategy
- `RagEvaluationAgent`:
  defines evaluation, hallucination controls, latency, and cost optimization approach

Those outputs are then synthesized into one final recommendation for the user.

## Prerequisites

- Python 3.12+
- `ANTHROPIC_API_KEY`

## Run

From the repository root:

```bash
ANTHROPIC_API_KEY=your-key-here python3 Orchestrators/Python/main.py --brief-file Orchestrators/Python/examples/rag-modernization-brief.md
```

From the runtime folder:

```bash
cd Orchestrators/Python
ANTHROPIC_API_KEY=your-key-here python3 -m aitrove_python --brief-file examples/rag-modernization-brief.md
```

## Input Modes

Provide the RAG problem statement one of three ways:

```bash
python3 Orchestrators/Python/main.py --brief-file Orchestrators/Python/examples/rag-modernization-brief.md
```

```bash
python3 -m aitrove_python --brief "Design a RAG platform for support agents across five product lines."
```

```bash
cat Orchestrators/Python/examples/rag-modernization-brief.md | python3 Orchestrators/Python/main.py --stdin
```

Optional metadata:

- `--corpus "Describe the corpus and source systems"`
- `--constraints "Latency, compliance, tenancy, and budget constraints"`
- `--prompt "Customize the synthesis request"`
- `--system-prompt "Customize the synthesis system prompt"`

## Model Selection

The runtime uses Anthropic’s official Sonnet model IDs and aliases as documented in Anthropic’s models overview:

- `claude-sonnet-4-20250514`
- `claude-sonnet-4-0`
- `claude-3-7-sonnet-latest`
- `claude-3-5-sonnet-latest`

To force a specific model for live runs, set:

```bash
AITROVE_ANTHROPIC_MODEL=claude-3-7-sonnet-latest
```

## Tests

Run the stdlib test suite from `Orchestrators/Python`:

```bash
python3 -m unittest discover tests -v
```

The tests cover:

- CLI request parsing
- agent prompt construction through a fake Anthropic client
- workflow synthesis behavior
