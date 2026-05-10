from __future__ import annotations

import logging
import unittest
from pathlib import Path

from aitrove_python.agent_context import AgentContext
from aitrove_python.agents.base import Agent
from aitrove_python.agents.rag_architecture_agent import RagArchitectureAgent
from aitrove_python.prompt_loader import FilePromptLoader
from aitrove_python.workflow_orchestrator import WorkflowOrchestrator


class WorkflowTests(unittest.IsolatedAsyncioTestCase):
    async def test_rag_architecture_agent_uses_brief_metadata(self) -> None:
        prompts_dir = Path(__file__).resolve().parents[1] / "prompts"
        anthropic = RecordingAnthropicClient("architecture-summary")
        agent = RagArchitectureAgent(FilePromptLoader(prompts_dir), anthropic)
        ctx = AgentContext(
            api_key="test-key",
            user_message="unused",
            system_prompt="unused",
            metadata={
                "rag_brief": "Design a RAG platform for support.",
                "corpus_context": "Docs and tickets",
                "constraints": "Low latency",
            },
        )

        result = await agent.execute(ctx)

        self.assertEqual("architecture-summary", result)
        call = anthropic.calls[0]
        self.assertIn("Design a RAG platform for support.", call["user_message"])
        self.assertIn("Docs and tickets", call["user_message"])
        self.assertIn("Low latency", call["user_message"])

    async def test_workflow_synthesizes_successful_agent_outputs(self) -> None:
        anthropic = RecordingAnthropicClient("final-summary")
        orchestrator = WorkflowOrchestrator(
            agents=[
                StubAgent("ArchitectureAgent", True, "Use hybrid retrieval."),
                StubAgent("EvaluationAgent", True, "Track grounded answer rate."),
                StubAgent("IgnoredAgent", False, "unused"),
            ],
            anthropic=anthropic,
            logger=logging.getLogger("test.workflow"),
        )
        ctx = AgentContext(
            api_key="test-key",
            user_message="Design the best RAG platform.",
            system_prompt="Synthesize the findings.",
            metadata={"rag_brief": "Support RAG"},
        )

        result = await orchestrator.run(ctx)

        self.assertEqual("final-summary", result)
        synthesis_call = anthropic.calls[-1]
        self.assertIn("### ArchitectureAgent findings", synthesis_call["user_message"])
        self.assertIn("### EvaluationAgent findings", synthesis_call["user_message"])
        self.assertIn("### User request", synthesis_call["user_message"])


class RecordingAnthropicClient:
    def __init__(self, response: str) -> None:
        self._response = response
        self.calls: list[dict[str, object]] = []

    async def send_message(
        self,
        api_key: str,
        requested_model: str,
        max_tokens: int,
        system_prompt: str,
        user_message: str,
    ) -> str:
        self.calls.append(
            {
                "api_key": api_key,
                "requested_model": requested_model,
                "max_tokens": max_tokens,
                "system_prompt": system_prompt,
                "user_message": user_message,
            }
        )
        return self._response


class StubAgent(Agent):
    def __init__(self, name: str, can_handle: bool, output: str) -> None:
        self._name = name
        self._can_handle = can_handle
        self._output = output

    @property
    def name(self) -> str:
        return self._name

    def can_handle(self, ctx: AgentContext) -> bool:
        return self._can_handle

    async def execute(self, ctx: AgentContext) -> str:
        return self._output
