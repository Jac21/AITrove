export type ManifestSection = {
  id: string;
  title: string;
  description: string;
};

export type ManifestEntry = {
  id: string;
  path: string;
  title: string;
  section: string;
  extension: string;
  language: string;
  summary: string;
  content: string;
  bytes: number;
};

export type RepositoryManifest = {
  generatedAt: string;
  sections: ManifestSection[];
  entries: ManifestEntry[];
};
