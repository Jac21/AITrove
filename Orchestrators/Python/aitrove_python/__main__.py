from __future__ import annotations

import asyncio
import logging
import os
import sys
from pathlib import Path

from .agent_context import AgentContext
from .anthropic_client import AnthropicMessageClient
from .cli_request import CliRequestParser
from .prompt_loader import FilePromptLoader
from .workflow_orchestrator import WorkflowOrchestrator
from .agents.rag_architecture_agent import RagArchitectureAgent
from .agents.rag_evaluation_agent import RagEvaluationAgent
from .agents.retrieval_strategy_agent import RetrievalStrategyAgent


async def main(argv: list[str] | None = None) -> int:
    argv = list(sys.argv[1:] if argv is None else argv)

    try:
        request = await CliRequestParser.parse(argv, sys.stdin, not sys.stdin.isatty())
    except ValueError as exc:
        print(str(exc), file=sys.stderr)
        print("", file=sys.stderr)
        print(CliRequestParser.build_usage(), file=sys.stderr)
        return 1

    api_key = os.getenv("ANTHROPIC_API_KEY")
    if not api_key:
        raise RuntimeError("ANTHROPIC_API_KEY not set")

    logging.basicConfig(level=logging.INFO, format="%(levelname)s: %(name)s: %(message)s")
    logger = logging.getLogger("aitrove_python.workflow")

    prompts_dir = Path(__file__).resolve().parents[1] / "prompts"
    prompt_loader = FilePromptLoader(prompts_dir)
    anthropic = AnthropicMessageClient()
    orchestrator = WorkflowOrchestrator(
        agents=[
            RagArchitectureAgent(prompt_loader, anthropic),
            RetrievalStrategyAgent(prompt_loader, anthropic),
            RagEvaluationAgent(prompt_loader, anthropic),
        ],
        anthropic=anthropic,
        logger=logger,
    )

    context = AgentContext(
        api_key=api_key,
        user_message=request.user_message,
        system_prompt=request.system_prompt,
        metadata=request.metadata,
    )

    result = await orchestrator.run(context)
    print("\n=== Synthesis ===")
    print(result)
    return 0


if __name__ == "__main__":
    raise SystemExit(asyncio.run(main()))
