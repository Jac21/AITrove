# AITrove

AITrove is a working repository for AI orchestration patterns, prompt assets, portable skills, and supporting developer instructions.

The repository is organized around reusable building blocks rather than a single application:

- orchestrators for multi-agent and synthesis workflows
- prompts for runtime behavior and task shaping
- Markdown skills that can be reused across platforms
- editor and agent instructions for different tooling environments
- a static front-end directory for browsing repository assets

## Repository Areas

| Path | Purpose |
| --- | --- |
| [`Orchestrators/`](/Users/jeremycantu/Downloads/Development/Github/AITrove/Orchestrators/README.md:1) | Runtime implementations for agent orchestration patterns |
| [`Prompts/`](/Users/jeremycantu/Downloads/Development/Github/AITrove/Prompts/README.md:1) | Shared prompt templates used by agents and orchestrators |
| [`Skills/`](/Users/jeremycantu/Downloads/Development/Github/AITrove/Skills/SKILL.md:1) | Cross-platform Markdown skill packs and references |
| [`Instructions/`](/Users/jeremycantu/Downloads/Development/Github/AITrove/Instructions/README.md:1) | Tool-specific working instructions for Claude, Copilot, and Cursor |
| [`Frontend/Directory/`](/Users/jeremycantu/Downloads/Development/Github/AITrove/Frontend/Directory/README.md:1) | Static GitHub Pages front-end for repository discovery |

## Current Runtimes

### .NET

The .NET runtime is the reference implementation for the fan-out and synthesis pattern.

- runtime:
  [`Orchestrators/DotNet/`](/Users/jeremycantu/Downloads/Development/Github/AITrove/Orchestrators/DotNet/README.md:1)
- tests:
  [`Orchestrators/DotNet.Tests/`](/Users/jeremycantu/Downloads/Development/Github/AITrove/Orchestrators/DotNet.Tests)

### Python

The Python runtime is oriented toward RAG system design and interview-relevant architecture analysis.

- runtime:
  [`Orchestrators/Python/`](/Users/jeremycantu/Downloads/Development/Github/AITrove/Orchestrators/Python/README.md:1)

## Interactive Directory

The repository includes a static React/Vite directory front-end intended for GitHub Pages deployment:

- source:
  [`Frontend/Directory/`](/Users/jeremycantu/Downloads/Development/Github/AITrove/Frontend/Directory/README.md:1)
- workflow:
  [`.github/workflows/deploy-directory-pages.yml`](/Users/jeremycantu/Downloads/Development/Github/AITrove/.github/workflows/deploy-directory-pages.yml:1)
- site:
  [https://jac21.github.io/AITrove/](https://jac21.github.io/AITrove/)

## Primary Patterns In This Repo

- Fan-out and synthesize:
  run specialized agents independently, then combine their outputs into one final answer
- Prompt-driven orchestration:
  keep task behavior in versioned prompt files instead of hardcoding instructions
- Portable skills:
  define reusable capabilities in Markdown so they can be applied across runtimes
- Static repository discovery:
  publish a searchable repository atlas through GitHub Pages

## Development Notes

- The repository currently contains working .NET and Python orchestrator implementations.
- The front-end directory app is static and deploys through GitHub Actions to GitHub Pages.
- Prompts and skills are treated as first-class assets alongside code.
