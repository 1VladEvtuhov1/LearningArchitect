# ExtractionRPG Decisions

Durable choices for the **product workspace**. Showcase-specific decisions remain in `UnityClient/Docs/Decisions.md`.

## D-WS-001: Repository root is a product workspace

The git root is **ExtractionRPG**, not a Unity project folder.

- Unity opens from **`UnityClient/`**
- Backend lives in **`Backend/`**
- Product docs in root **`Docs/`**
- Portfolio shell stays at **`Site/`**

Rationale: one repo for WebGL client + API + deployment docs without nesting backend inside `Assets/`.

## D-WS-002: Unity docs stay under UnityClient

Historical documentation for showcase and Interview Arena moved to **`UnityClient/Docs/`**.

Paths inside those files refer to `Assets/...` relative to **`UnityClient/`** unless explicitly prefixed.

Root **`Docs/`** holds client/server product narrative only.

## D-WS-003: Site remains at repository root

`Site/` is not moved into `UnityClient/` because it hosts the WebGL **build output** and static portfolio page independent of Unity project structure.

## D-WS-004: Backend migration is scaffold-only first

First backend PR after workspace migration: folders + README + docker-compose — **no** API business logic.

## Inherited product decisions (summary)

| ID | Topic | Location |
|----|--------|----------|
| D-001 | Single showcase entry scene | `UnityClient/Docs/Decisions.md` |
| D-008 | Interview Arena = separate scene + hub dock | `UnityClient/Docs/Decisions.md` |

See full list in `UnityClient/Docs/Decisions.md`.
