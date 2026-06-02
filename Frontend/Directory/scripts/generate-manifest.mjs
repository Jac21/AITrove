import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const appRoot = path.resolve(__dirname, "..");
const repoRoot = path.resolve(appRoot, "..", "..");
const outputPath = path.join(appRoot, "public", "generated", "manifest.json");
const conceptsPath = path.join(appRoot, "content", "concepts.json");

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
  const concepts = await loadConcepts();
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
  validateConceptCoverage(entries, concepts);

  const manifest = {
    generatedAt: new Date().toISOString(),
    sections: sections.map(({ id, title, description }) => ({ id, title, description })),
    concepts,
    entries,
  };

  await fs.mkdir(path.dirname(outputPath), { recursive: true });
  await fs.writeFile(outputPath, JSON.stringify(manifest, null, 2));
  console.log(
    `Generated manifest with ${entries.length} entries and ${concepts.length} concepts at ${path.relative(repoRoot, outputPath)}`,
  );
}

async function loadConcepts() {
  const content = await fs.readFile(conceptsPath, "utf8");
  const concepts = JSON.parse(content);

  if (!Array.isArray(concepts)) {
    throw new Error("Directory concepts must be a JSON array.");
  }

  const sectionIds = new Set(sections.map((section) => section.id));
  const conceptIds = new Set();

  return concepts.map((concept, index) => {
    const location = `concepts[${index}]`;
    assertNonEmptyString(concept.id, `${location}.id`);
    assertNonEmptyString(concept.title, `${location}.title`);
    assertNonEmptyString(concept.section, `${location}.section`);
    assertNonEmptyString(concept.summary, `${location}.summary`);
    assertNonEmptyString(concept.details, `${location}.details`);
    assertStringArray(concept.relatedPaths, `${location}.relatedPaths`);
    assertStringArray(concept.tags, `${location}.tags`);

    if (conceptIds.has(concept.id)) {
      throw new Error(`Duplicate directory concept id: ${concept.id}`);
    }

    if (!sectionIds.has(concept.section)) {
      throw new Error(`Directory concept ${concept.id} uses unknown section: ${concept.section}`);
    }

    conceptIds.add(concept.id);

    return {
      id: concept.id,
      title: concept.title,
      section: concept.section,
      summary: concept.summary,
      details: concept.details,
      relatedPaths: concept.relatedPaths,
      tags: concept.tags,
    };
  });
}

function validateConceptCoverage(entries, concepts) {
  const explainedPaths = new Set(concepts.flatMap((concept) => concept.relatedPaths));
  const missing = entries.filter((entry) => requiresConcept(entry.path) && !explainedPaths.has(entry.path));

  if (missing.length === 0) {
    return;
  }

  const formattedPaths = missing.map((entry) => `- ${entry.path}`).join("\n");
  throw new Error(
    [
      "The directory site is missing concept explanations for complex repository constructs.",
      "Add entries to Frontend/Directory/content/concepts.json with relatedPaths for:",
      formattedPaths,
    ].join("\n"),
  );
}

function requiresConcept(relativePath) {
  if (relativePath === "Orchestrators/README.md") {
    return true;
  }

  if (/^Orchestrators\/[^/]+\/README\.md$/.test(relativePath)) {
    return true;
  }

  if (/^Orchestrators\/Markdown\/[^/]+\/(README|workflow)\.md$/.test(relativePath)) {
    return true;
  }

  return /^Skills\/[^/]+\/SKILL\.md$/.test(relativePath);
}

function assertNonEmptyString(value, fieldName) {
  if (typeof value !== "string" || value.trim().length === 0) {
    throw new Error(`Directory concept field ${fieldName} must be a non-empty string.`);
  }
}

function assertStringArray(value, fieldName) {
  if (!Array.isArray(value) || value.some((item) => typeof item !== "string" || item.trim().length === 0)) {
    throw new Error(`Directory concept field ${fieldName} must be an array of non-empty strings.`);
  }
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
