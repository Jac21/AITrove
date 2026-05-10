from __future__ import annotations

from pathlib import Path


class FilePromptLoader:
    def __init__(self, prompts_directory: Path) -> None:
        self._prompts_directory = prompts_directory

    async def load(self, name: str) -> str:
        path = self._prompts_directory / f"{name}.md"
        if not path.exists():
            raise FileNotFoundError(f"Prompt template not found: {path}")
        return path.read_text(encoding="utf-8")
