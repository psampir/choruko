# Choruko frontend

React 19 + TypeScript + Vite + SCSS. Part of the [Choruko](../../README.md) project.

## Development

```bash
npm install
npm run dev      # http://localhost:3000, proxies /api and /uploads to :5180
npm run build    # tsc -b && vite build
npm run lint
```

See the [development guide](../../docs/guides/set-up-dev-env.md) for the full setup,
including the backend and database.

## Layout

- `src/views` — route screens, lazy-loaded from `App.tsx`
- `src/contexts` — per-domain state; call a service, then refresh
- `src/services` — API clients (`fetch`, cookie auth)
- `src/components` — shared UI, one folder per component
- `src/components/InstallHint` — add-to-home-screen (iOS) and Android app hint
