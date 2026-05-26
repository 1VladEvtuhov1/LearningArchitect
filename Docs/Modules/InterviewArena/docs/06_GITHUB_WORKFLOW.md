# 06. GitHub Workflow

## Branch Strategy

Simple strategy for solo development:

```text
main
feature/player-movement
feature/lobby
feature/backend-auth
feature/webgl-build
fix/lobby-ready-state
docs/readme-update
```

`main` should stay runnable.

## Commit Style

Use clear commit messages:

```text
feat(player): add rigidbody movement prototype
feat(lobby): add ready state model
feat(auth): implement username login endpoint
fix(physics): stabilize ground detection
docs(readme): describe server hosting flow
refactor(core): split bootstrap and scene loading
```

Recommended prefixes:

```text
feat
fix
docs
refactor
test
chore
perf
```

## Pull Request Template

Even for solo work, PRs are useful for documentation.

```md
## What changed

## Why

## How to test

## Interview topics covered

## Notes / trade-offs
```

## Issue Template

```md
## Goal

## Tasks

- [ ] 

## Acceptance Criteria

- [ ] 

## Related Interview Topics
```

## Unity Gitignore Notes

Commit:

- `Assets/`
- `Packages/`
- `ProjectSettings/`
- `UserSettings/` optional, usually skip
- `docs/`
- `.cursor/`

Do not commit:

- `Library/`
- `Temp/`
- `Obj/`
- `Build/`
- `Builds/`
- `Logs/`
- `.vs/`
- `.idea/`

## Recommended First Commits

1. `chore(repo): add initial Unity project`
2. `docs(project): add project documentation`
3. `chore(cursor): add Cursor rules`
4. `feat(core): add bootstrap scene`
5. `feat(player): add movement prototype`

## GitHub README Checklist

The README should answer:

- What is this?
- Why does it exist?
- How do I run it?
- What systems are implemented?
- What Unity/C# topics does it demonstrate?
- What are the current limitations?
- What is planned next?
