from __future__ import annotations

from dataclasses import dataclass, field


@dataclass(slots=True)
class AgentContext:
    api_key: str
    user_message: str
    system_prompt: str
    metadata: dict[str, str] = field(default_factory=dict)
    agent_results: list[tuple[str, str]] = field(default_factory=list)

    def add_agent_result(self, name: str, output: str) -> None:
        self.agent_results.append((name, output))

    def build_synthesis_prompt(self) -> str:
        parts: list[str] = []

        for name, output in self.agent_results:
            parts.append(f"### {name} findings")
            parts.append(output)
            parts.append("")

        parts.append("### User request")
        parts.append(self.user_message)
        return "\n".join(parts)
