# Bright Path — React UI

React 18 + TypeScript (Vite) room board: six rooms grouped by room, per-lesson
conflict codes, post-cut-off changes, and the per-tutor load line.

## Run

Two shells:

```bash
cd backend
dotnet run --project Api
```

```bash
cd frontend
npm install
npm run dev
```

Then open http://localhost:5173. The Vite dev server proxies `/api/*` to the .NET
API at `http://localhost:5243` (see `vite.config.ts`), so no CORS config is needed.

## Pinned "today"

The board defaults to **2026-03-06** (`PINNED_TODAY` in `src/api/schedule.ts`),
matching `Schedule:Now` in `backend/Api/appsettings.json`. `GET /api/schedule` requires an
explicit `date`, so this constant is always sent.

## Build

```bash
npm run build
```

Outputs a static bundle to `frontend/dist/` (not wired into the API — serve it
with any static host).

## Structure

- `src/api/` — DTO types (`types.ts`) and the read fetches (`schedule.ts`,
  `tutors.ts`, `conflicts.ts`)
- `src/lib/format.ts` — `formatTime()` helper (`"HH:mm:ss"` -> `"HH:mm"`)
- `src/components/` — one component per board section: `App`, `Banner`,
  `Changes`, `Loads`, `Rooms`, `Lesson`
- `src/styles.css` — the board's styles
