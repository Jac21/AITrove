import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const appRoot = path.resolve(__dirname, "..");
const repoRoot = path.resolve(appRoot, "..", "..");
const outputPath = path.join(appRoot, "public", "generated", "manifest.json");

const sections = [
  {
    id: "overview",
    title: "Overview",
    description: "Top-level repository documents and orientation points.",
    roots: ["README.md", "LICENSE"],
  },
  {
    id: "instructions",
    title: "Instructions",
    description: "Editor, agent, and tooling conventions for working in the repository.",
    roots: ["Instructions"],
  },
  {
    id: "orchestrators",
    title: "Orchestrators",
    description: "Runtime implementations, prompts, and tests for orchestrated AI workflows.",
    roots: ["Orchestrators"],
  },
  {
    id: "prompts",
    title: "Prompts",
    description: "Shared prompt templates used by agents and orchestrators.",
    roots: ["Prompts"],
  },
  {
    id: "skills",
    title: "Skills",
    description: "Portable Markdown skill packs and supporting references.",
    roots: ["Skills"],
  },
];

const textExtensions = new Set([
  ".md",
  ".mdc",
  ".txt",
  ".cs",
  ".csproj",
  ".py",
  ".toml",
  ".json",
  ".yml",
  ".yaml",
  ".mjs",
  ".ts",
  ".tsx",
  ".html",
  ".css",
  ".sln",
  ".xml",
]);

const ignoredNames = new Set([
  ".git",
  ".idea",
  ".DS_Store",
  "bin",
  "obj",
  "node_modules",
  "__pycache__",
  "dist",
  "build",
]);

const extensionToLanguage = {
  ".md": "Markdown",
  ".mdc": "Markdown",
  ".txt": "Text",
  ".cs": "C#",
  ".csproj": "MSBuild",
  ".py": "Python",
  ".toml": "TOML",
  ".json": "JSON",
  ".yml": "YAML",
  ".yaml": "YAML",
  ".mjs": "JavaScript",
  ".ts": "TypeScript",
  ".tsx": "TSX",
  ".html": "HTML",
  ".css": "CSS",
  ".sln": "Solution",
  ".xml": "XML",
};

async function main() {
  const entries = [];

  for (const section of sections) {
    for (const root of section.roots) {
      const absoluteRoot = path.join(repoRoot, root);
      const stat = await safeStat(absoluteRoot);
      if (!stat) {
        continue;
      }

      if (stat.isDirectory()) {
        await walkDirectory(absoluteRoot, section, entries);
      } else if (stat.isFile()) {
        const entry = await createEntry(absoluteRoot, section);
        if (entry) {
          entries.push(entry);
        }
      }
    }
  }

  entries.sort((left, right) => left.path.localeCompare(right.path));

  const manifest = {
    generatedAt: new Date().toISOString(),
    sections: sections.map(({ id, title, description }) => ({ id, title, description })),
    entries,
  };

  await fs.mkdir(path.dirname(outputPath), { recursive: true });
  await fs.writeFile(outputPath, JSON.stringify(manifest, null, 2));
  console.log(`Generated manifest with ${entries.length} entries at ${path.relative(repoRoot, outputPath)}`);
}

async function walkDirectory(directory, section, entries) {
  const children = await fs.readdir(directory, { withFileTypes: true });

  for (const child of children) {
    if (ignoredNames.has(child.name)) {
      continue;
    }

    const absolutePath = path.join(directory, child.name);

    if (child.isDirectory()) {
      await walkDirectory(absolutePath, section, entries);
      continue;
    }

    if (child.isFile()) {
      const entry = await createEntry(absolutePath, section);
      if (entry) {
        entries.push(entry);
      }
    }
  }
}

async function createEntry(absolutePath, section) {
  const extension = path.extname(absolutePath).toLowerCase();
  if (!textExtensions.has(extension)) {
    return null;
  }

  const content = await fs.readFile(absolutePath, "utf8");
  const relativePath = path.relative(repoRoot, absolutePath).replaceAll(path.sep, "/");
  const title = path.basename(absolutePath);

  return {
    id: relativePath.replaceAll("/", "__"),
    path: relativePath,
    title,
    section: section.id,
    extension,
    language: extensionToLanguage[extension] ?? "Text",
    summary: summarizeContent(content),
    content,
    bytes: Buffer.byteLength(content, "utf8"),
  };
}

function summarizeContent(content) {
  for (const rawLine of content.split(/\r?\n/)) {
    const line = rawLine
      .replace(/^#+\s*/, "")
      .replace(/^[-*]\s*/, "")
      .replace(/^\/\/\/?\s*/, "")
      .trim();

    if (line.length > 0) {
      return line.slice(0, 140);
    }
  }

  return "No summary available.";
}

async function safeStat(targetPath) {
  try {
    return await fs.stat(targetPath);
  } catch {
    return null;
  }
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
