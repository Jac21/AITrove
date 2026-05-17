import { useEffect, useMemo, useState } from "react";
import type { ManifestEntry, ManifestSection, RepositoryManifest } from "./types";

const manifestUrl = `${import.meta.env.BASE_URL}generated/manifest.json`;

export default function App() {
  const [manifest, setManifest] = useState<RepositoryManifest | null>(null);
  const [query, setQuery] = useState("");
  const [activeSection, setActiveSection] = useState<string>("all");
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    void (async () => {
      try {
        const response = await fetch(manifestUrl);
        if (!response.ok) {
          throw new Error(`Failed to load manifest: ${response.status}`);
        }

        const data = (await response.json()) as RepositoryManifest;
        setManifest(data);
        setSelectedId(data.entries[0]?.id ?? null);
      } catch (loadError) {
        const message = loadError instanceof Error ? loadError.message : "Unknown error";
        setError(message);
      }
    })();
  }, []);

  const sections = manifest?.sections ?? [];
  const entries = manifest?.entries ?? [];

  const filteredEntries = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    return entries.filter((entry) => {
      const matchesSection = activeSection === "all" || entry.section === activeSection;
      if (!matchesSection) {
        return false;
      }

      if (!normalizedQuery) {
        return true;
      }

      const haystack = `${entry.title} ${entry.path} ${entry.summary} ${entry.language}`.toLowerCase();
      return haystack.includes(normalizedQuery);
    });
  }, [activeSection, entries, query]);

  useEffect(() => {
    if (!filteredEntries.some((entry) => entry.id === selectedId)) {
      setSelectedId(filteredEntries[0]?.id ?? null);
    }
  }, [filteredEntries, selectedId]);

  const selectedEntry = filteredEntries.find((entry) => entry.id === selectedId) ?? filteredEntries[0] ?? null;
  const activeSectionMeta = sections.find((section) => section.id === activeSection) ?? null;

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand-block">
          <p className="eyebrow">AITrove</p>
          <h1>Interactive Directory</h1>
          <p className="lede">
            Static repository atlas for prompts, skills, orchestrators, and working instructions.
          </p>
        </div>

        <label className="search-block">
          <span>Search repository content</span>
          <input
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            placeholder="Search by file, path, or summary"
          />
        </label>

        <nav className="section-list" aria-label="Repository sections">
          <SectionButton
            title="All Sections"
            description="Everything included in the generated manifest."
            count={entries.length}
            active={activeSection === "all"}
            onClick={() => setActiveSection("all")}
          />
          {sections.map((section) => (
            <SectionButton
              key={section.id}
              title={section.title}
              description={section.description}
              count={entries.filter((entry) => entry.section === section.id).length}
              active={activeSection === section.id}
              onClick={() => setActiveSection(section.id)}
            />
          ))}
        </nav>
      </aside>

      <main className="content-shell">
        <section className="catalog-panel">
          <header className="panel-header">
            <div>
              <p className="eyebrow">Directory</p>
              <h2>{activeSectionMeta?.title ?? "All Sections"}</h2>
              <p className="panel-copy">
                {activeSectionMeta?.description ?? "Cross-section view of the repository structure."}
              </p>
            </div>
            <div className="meta-pill">{filteredEntries.length} files</div>
          </header>

          {error ? <ErrorState message={error} /> : null}

          {!error && !manifest ? <EmptyState title="Loading manifest" body="Building the repository view." /> : null}

          {!error && manifest && filteredEntries.length === 0 ? (
            <EmptyState title="No matching files" body="Adjust the search query or switch sections." />
          ) : null}

          <div className="entry-grid">
            {filteredEntries.map((entry, index) => (
              <article
                key={entry.id}
                className={`entry-card ${selectedEntry?.id === entry.id ? "is-selected" : ""}`}
                style={{ animationDelay: `${index * 35}ms` }}
                onClick={() => setSelectedId(entry.id)}
              >
                <div className="entry-card-header">
                  <span className="file-chip">{entry.language}</span>
                  <span className="size-chip">{formatBytes(entry.bytes)}</span>
                </div>
                <h3>{entry.title}</h3>
                <p className="entry-path">{entry.path}</p>
                <p className="entry-summary">{entry.summary}</p>
              </article>
            ))}
          </div>
        </section>

        <section className="detail-panel">
          {selectedEntry ? (
            <DetailView entry={selectedEntry} section={sections.find((item) => item.id === selectedEntry.section) ?? null} />
          ) : (
            <EmptyState title="No file selected" body="Select a file from the directory to inspect its contents." />
          )}
        </section>
      </main>
    </div>
  );
}

function SectionButton(props: {
  title: string;
  description: string;
  count: number;
  active: boolean;
  onClick: () => void;
}) {
  return (
    <button className={`section-button ${props.active ? "is-active" : ""}`} onClick={props.onClick}>
      <span className="section-button-topline">
        <strong>{props.title}</strong>
        <span>{props.count}</span>
      </span>
      <span className="section-button-copy">{props.description}</span>
    </button>
  );
}

function DetailView(props: { entry: ManifestEntry; section: ManifestSection | null }) {
  const [copied, setCopied] = useState<"path" | "content" | null>(null);

  async function copy(value: string, type: "path" | "content") {
    await navigator.clipboard.writeText(value);
    setCopied(type);
    window.setTimeout(() => setCopied(null), 1400);
  }

  return (
    <>
      <header className="panel-header detail-header">
        <div>
          <p className="eyebrow">{props.section?.title ?? "File"}</p>
          <h2>{props.entry.title}</h2>
          <p className="panel-copy">{props.entry.summary}</p>
        </div>
        <div className="detail-actions">
          <button onClick={() => void copy(props.entry.path, "path")}>{copied === "path" ? "Copied path" : "Copy path"}</button>
          <button onClick={() => void copy(props.entry.content, "content")}>
            {copied === "content" ? "Copied content" : "Copy content"}
          </button>
        </div>
      </header>

      <div className="detail-meta">
        <span>{props.entry.path}</span>
        <span>{props.entry.language}</span>
        <span>{formatBytes(props.entry.bytes)}</span>
      </div>

      <pre className="content-preview">
        <code>{props.entry.content}</code>
      </pre>
    </>
  );
}

function ErrorState(props: { message: string }) {
  return (
    <div className="state-card is-error">
      <h3>Manifest load failed</h3>
      <p>{props.message}</p>
    </div>
  );
}

function EmptyState(props: { title: string; body: string }) {
  return (
    <div className="state-card">
      <h3>{props.title}</h3>
      <p>{props.body}</p>
    </div>
  );
}

function formatBytes(bytes: number) {
  if (bytes < 1024) {
    return `${bytes} B`;
  }

  const kibibytes = bytes / 1024;
  if (kibibytes < 1024) {
    return `${kibibytes.toFixed(1)} KB`;
  }

  return `${(kibibytes / 1024).toFixed(1)} MB`;
}
