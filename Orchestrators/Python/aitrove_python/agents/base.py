from __future__ import annotations

from abc import ABC, abstractmethod

from ..agent_context import AgentContext


class Agent(ABC):
    @property
    @abstractmethod
    def name(self) -> str:
        raise NotImplementedError

    @abstractmethod
    def can_handle(self, ctx: AgentContext) -> bool:
        raise NotImplementedError

    @abstractmethod
    async def execute(self, ctx: AgentContext) -> str:
        raise NotImplementedError
