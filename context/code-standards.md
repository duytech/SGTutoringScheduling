# Code standards — Bright Path Learning Centre

Referenced from `CLAUDE.md`. Naming and API-design conventions live here — follow
them for all code in this repo.

## General
- Keep modules, API, class, function small and single-purpose.

## Engineering Mindset
- **Every feature must be testable** — if it cannot be verified immediately after implementation, it is incomplete
- **Clean over clever** — simple readable code that a junior developer can understand is always preferred over clever abstractions
- **One thing at a time** — complete one feature fully before touching the next
- **Failures are expected** — wrap agent operations in try/catch, log failures, never let one failure crash everything

## Naming
- Spell names out in full, no abbreviations.
- No single-character names, loop indices included.
- Names in your code should tell you what they do.
- Avoid Magic Numbers and Strings.
- Keep a blank line between a closing brace and the statement that follows it (so an early `return` after a guard block stands apart).

## API design
- Follow Entity-oriented API design.
- One endpoint per responsibility. When a response starts carrying an unrelated concern, split it into its own endpoint/resource instead of growing the payload.
