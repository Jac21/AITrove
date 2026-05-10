from __future__ import annotations

import unittest
from io import StringIO
from pathlib import Path
from tempfile import NamedTemporaryFile

from aitrove_python.cli_request import CliRequestParser


class CliRequestParserTests(unittest.IsolatedAsyncioTestCase):
    async def test_parse_uses_inline_brief(self) -> None:
        request = await CliRequestParser.parse(
            ["--brief", "Design a RAG platform", "--corpus", "Docs and tickets"],
            StringIO(""),
            is_input_redirected=False,
        )

        self.assertEqual("Design a RAG platform", request.brief)
        self.assertEqual("Docs and tickets", request.metadata["corpus_context"])

    async def test_parse_reads_brief_file(self) -> None:
        with NamedTemporaryFile("w", encoding="utf-8", delete=False) as handle:
            handle.write("Design a support RAG system")
            path = Path(handle.name)

        try:
            request = await CliRequestParser.parse(
                ["--brief-file", str(path), "--constraints", "Low latency"],
                StringIO(""),
                is_input_redirected=False,
            )
        finally:
            path.unlink(missing_ok=True)

        self.assertIn("support RAG system", request.brief)
        self.assertEqual("Low latency", request.metadata["constraints"])

    async def test_parse_reads_redirected_stdin(self) -> None:
        request = await CliRequestParser.parse(
            [],
            StringIO("Design a multi-tenant RAG platform"),
            is_input_redirected=True,
        )

        self.assertIn("multi-tenant RAG platform", request.brief)
