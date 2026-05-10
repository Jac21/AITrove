from __future__ import annotations

from ..agent_context import AgentContext
from ..anthropic_client import AnthropicMessageClient, AnthropicModelIds
from ..prompt_loader import FilePromptLoader
from .base import Agent


class RagEvaluationAgent(Agent):
    def __init__(self, prompts: FilePromptLoader, anthropic: AnthropicMessageClient) -> None:
        self._prompts = prompts
        self._anthropic = anthropic

    @property
    def name(self) -> str:
        return "RagEvaluationAgent"

    def can_handle(self, ctx: AgentContext) -> bool:
        return "rag_brief" in ctx.metadata

    async def execute(self, ctx: AgentContext) -> str:
        system_prompt = await self._prompts.load("rag-evaluation.system")
        user_template = await self._prompts.load("rag-evaluation.user")
        user_message = (
            user_template
            .replace("{{BRIEF}}", ctx.metadata.get("rag_brief", ""))
            .replace("{{CORPUS_CONTEXT}}", ctx.metadata.get("corpus_context", "Not provided."))
            .replace("{{CONSTRAINTS}}", ctx.metadata.get("constraints", "Not provided."))
        )

        return await self._anthropic.send_message(
            api_key=ctx.api_key,
            requested_model=AnthropicModelIds.CLAUDE_SONNET_4,
            max_tokens=900,
            system_prompt=system_prompt,
            user_message=user_message,
        )
