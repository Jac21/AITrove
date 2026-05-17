# AITrove Directory Front-End

Static React/Vite front-end for browsing the repository as an interactive directory.

## What it does

- generates a static manifest from key repository folders
- groups content by section:
  `Overview`, `Instructions`, `Orchestrators`, `Prompts`, and `Skills`
- supports section filtering and search
- previews file contents directly in the browser

## Local development

From the app folder:

```bash
npm install
npm run dev
```

The dev server will rebuild the static app. The repository manifest is generated on `npm run build`.

## Production build

```bash
npm run build
```

That runs the manifest generator and emits the static site to `dist/`.

## GitHub Pages deployment

Deployment is handled by `.github/workflows/deploy-directory-pages.yml`.

Repository settings still need:

1. `Settings` → `Pages`
2. `Build and deployment` → `Source`
3. select `GitHub Actions`

After that, pushes to `main` touching the front-end or indexed repository folders will publish the site.
