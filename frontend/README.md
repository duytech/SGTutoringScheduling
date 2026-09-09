# Bright Path — React UI

React 18 + TypeScript (Vite) port of the room board. Same interface and logic as
the original static page at `Api/wwwroot/index.html`, which is left in place.

## Run

Two shells:

```bash
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
matching `Schedule:Now` in `Api/appsettings.json`. `GET /api/schedule` requires an
explicit `date`, so this constant is always sent.

## Build

```bash
npm run build
```

Outputs a static bundle to `frontend/dist/` (not wired into the API — the original
page still serves at `http://localhost:5243/`).

## Structure

- `src/api/` — DTO types (`types.ts`) and the read fetches (`schedule.ts`,
  `tutors.ts`)
- `src/lib/format.ts` — `fmt()` time helper (`"HH:mm:ss"` -> `"HH:mm"`)
- `src/components/` — one component per original render function: `App`, `Banner`,
  `Changes`, `Loads`, `Rooms`, `Lesson`
- `src/styles.css` — copied verbatim from the original page
