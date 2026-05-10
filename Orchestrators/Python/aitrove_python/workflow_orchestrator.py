from __future__ import annotations

import asyncio
import logging
from dataclasses import dataclass

from .agent_context import AgentContext
from .anthropic_client import AnthropicMessageClient, AnthropicModelIds
from .agents.base import Agent


@dataclass(slots=True)
class AgentResult:
    agent_name: str
    output: str
    success: bool


class WorkflowOrchestrator:
    def __init__(
        self,
        agents: list[Agent],
        anthropic: AnthropicMessageClient,
        logger: logging.Logger,
    ) -> None:
        self._agents = list(agents)
        self._anthropic = anthropic
        self._logger = logger

    async def run(self, ctx: AgentContext) -> str:
        eligible = [agent for agent in self._agents if agent.can_handle(ctx)]
        self._logger.info("Workflow starting. Eligible agents: %s", len(eligible))

        if not eligible:
            self._logger.warning("No eligible agents found. Passing user message directly to Anthropic.")
            return await self._synthesize(ctx)

        results = await asyncio.gather(*(self._run_agent_safe(agent, ctx) for agent in eligible))

        for result in results:
            if result.success:
                ctx.add_agent_result(result.agent_name, result.output)

        self._logger.info("%s/%s agents succeeded", sum(1 for result in results if result.success), len(results))
        return await self._synthesize(ctx)

    async def _run_agent_safe(self, agent: Agent, ctx: AgentContext) -> AgentResult:
        try:
            self._logger.debug("Running agent: %s", agent.name)
            output = await agent.execute(ctx)
            return AgentResult(agent.name, output, True)
        except Exception:
            self._logger.exception("Agent %s failed — skipping", agent.name)
            return AgentResult(agent.name, "", False)

    async def _synthesize(self, ctx: AgentContext) -> str:
        return await self._anthropic.send_message(
            api_key=ctx.api_key,
            requested_model=AnthropicModelIds.CLAUDE_SONNET_4,
            max_tokens=1200,
            system_prompt=ctx.system_prompt,
            user_message=ctx.build_synthesis_prompt(),
        )
