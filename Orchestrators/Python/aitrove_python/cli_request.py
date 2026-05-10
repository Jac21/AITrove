from __future__ import annotations

import argparse
from dataclasses import dataclass, field
from pathlib import Path
from typing import TextIO


@dataclass(slots=True)
class CliRequest:
    brief: str
    user_message: str
    system_prompt: str
    metadata: dict[str, str] = field(default_factory=dict)


class CliRequestParser:
    DEFAULT_USER_MESSAGE = (
        "Design a production-ready RAG approach and summarize the highest-priority architecture, retrieval, and evaluation recommendations."
    )
    DEFAULT_SYSTEM_PROMPT = (
        "You are a principal AI engineer. Synthesize the agent findings into a concise, implementation-oriented RAG recommendation."
    )

    @classmethod
    async def parse(cls, argv: list[str], stdin: TextIO, is_input_redirected: bool) -> CliRequest:
        parser = cls._build_parser()
        args = parser.parse_args(argv)

        brief_sources = int(bool(args.brief)) + int(bool(args.brief_file)) + int(bool(args.stdin))
        if brief_sources > 1:
            raise ValueError("Choose only one brief input source: --brief, --brief-file, or --stdin.")

        if args.brief:
            brief = args.brief
        elif args.brief_file:
            brief = Path(args.brief_file).read_text(encoding="utf-8")
        elif args.stdin or is_input_redirected:
            brief = stdin.read()
            if not brief.strip():
                raise ValueError("No brief was provided on stdin.")
        else:
            raise ValueError("No brief input was provided. Use --brief, --brief-file, or --stdin.")

        metadata: dict[str, str] = {"rag_brief": brief}
        if args.corpus:
            metadata["corpus_context"] = args.corpus
        if args.constraints:
            metadata["constraints"] = args.constraints

        return CliRequest(
            brief=brief,
            user_message=args.prompt or cls.DEFAULT_USER_MESSAGE,
            system_prompt=args.system_prompt or cls.DEFAULT_SYSTEM_PROMPT,
            metadata=metadata,
        )

    @staticmethod
    def build_usage() -> str:
        return (
            "Usage:\n"
            "  python3 -m aitrove_python --brief-file examples/rag-modernization-brief.md\n"
            "  python3 -m aitrove_python --brief \"Design a RAG platform for support teams\"\n"
            "  cat examples/rag-modernization-brief.md | python3 -m aitrove_python --stdin\n"
        )

    @staticmethod
    def _build_parser() -> argparse.ArgumentParser:
        parser = argparse.ArgumentParser(add_help=True)
        parser.add_argument("--brief")
        parser.add_argument("--brief-file")
        parser.add_argument("--stdin", action="store_true")
        parser.add_argument("--corpus")
        parser.add_argument("--constraints")
        parser.add_argument("--prompt")
        parser.add_argument("--system-prompt")
        return parser
