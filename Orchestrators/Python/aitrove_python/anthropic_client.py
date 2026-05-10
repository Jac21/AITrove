from __future__ import annotations

import asyncio
import json
import os
import urllib.error
import urllib.request


class AnthropicModelIds:
    MODEL_OVERRIDE_ENVIRONMENT_VARIABLE = "AITROVE_ANTHROPIC_MODEL"

    CLAUDE_SONNET_4 = "claude-sonnet-4-20250514"
    CLAUDE_SONNET_4_ALIAS = "claude-sonnet-4-0"
    CLAUDE_SONNET_37_LATEST = "claude-3-7-sonnet-latest"
    CLAUDE_SONNET_35_LATEST = "claude-3-5-sonnet-latest"

    @classmethod
    def resolve_model_candidates(cls, requested_model: str) -> list[str]:
        override_model = os.getenv(cls.MODEL_OVERRIDE_ENVIRONMENT_VARIABLE)
        if override_model:
            return [override_model.strip()]

        if requested_model == cls.CLAUDE_SONNET_4:
            return [
                cls.CLAUDE_SONNET_4,
                cls.CLAUDE_SONNET_4_ALIAS,
                cls.CLAUDE_SONNET_37_LATEST,
                cls.CLAUDE_SONNET_35_LATEST,
            ]
        if requested_model == cls.CLAUDE_SONNET_4_ALIAS:
            return [
                cls.CLAUDE_SONNET_4_ALIAS,
                cls.CLAUDE_SONNET_37_LATEST,
                cls.CLAUDE_SONNET_35_LATEST,
            ]
        if requested_model == cls.CLAUDE_SONNET_37_LATEST:
            return [cls.CLAUDE_SONNET_37_LATEST, cls.CLAUDE_SONNET_35_LATEST]
        if requested_model == cls.CLAUDE_SONNET_35_LATEST:
            return [cls.CLAUDE_SONNET_35_LATEST]

        return [requested_model, cls.CLAUDE_SONNET_37_LATEST, cls.CLAUDE_SONNET_35_LATEST]


class AnthropicMessageClient:
    async def send_message(
        self,
        api_key: str,
        requested_model: str,
        max_tokens: int,
        system_prompt: str,
        user_message: str,
    ) -> str:
        last_model_not_found: Exception | None = None
        candidates = AnthropicModelIds.resolve_model_candidates(requested_model)

        for model in candidates:
            try:
                return await asyncio.to_thread(
                    self._send_single_message,
                    api_key,
                    model,
                    max_tokens,
                    system_prompt,
                    user_message,
                )
            except urllib.error.HTTPError as exc:
                error_text = exc.read().decode("utf-8", errors="replace")
                if self._is_model_not_found(error_text):
                    last_model_not_found = RuntimeError(error_text)
                    continue
                raise RuntimeError(error_text) from exc

        if last_model_not_found is not None:
            attempted = ", ".join(candidates)
            raise RuntimeError(
                f"Anthropic model lookup failed after trying: {attempted}. "
                f"Set {AnthropicModelIds.MODEL_OVERRIDE_ENVIRONMENT_VARIABLE} to a model available to your account. "
                f"Last error: {last_model_not_found}"
            ) from last_model_not_found

        raise RuntimeError("No Anthropic model candidates were resolved.")

    def _send_single_message(
        self,
        api_key: str,
        model: str,
        max_tokens: int,
        system_prompt: str,
        user_message: str,
    ) -> str:
        body = {
            "model": model,
            "max_tokens": max_tokens,
            "system": system_prompt,
            "messages": [
                {
                    "role": "user",
                    "content": [{"type": "text", "text": user_message}],
                }
            ],
        }

        request = urllib.request.Request(
            url="https://api.anthropic.com/v1/messages",
            data=json.dumps(body).encode("utf-8"),
            headers={
                "content-type": "application/json",
                "x-api-key": api_key,
                "anthropic-version": "2023-06-01",
            },
            method="POST",
        )

        with urllib.request.urlopen(request) as response:
            payload = json.loads(response.read().decode("utf-8"))

        content = payload.get("content", [])
        text_blocks = [item.get("text", "") for item in content if item.get("type") == "text"]
        return "\n".join(part for part in text_blocks if part).strip()

    @staticmethod
    def _is_model_not_found(error_text: str) -> bool:
        lowered = error_text.lower()
        return '"type":"not_found_error"' in lowered and "model:" in lowered
