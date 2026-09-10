# Code standards — Bright Path Learning Centre

Referenced from `CLAUDE.md`. Naming and API-design conventions live here — follow
them for all code in this repo.

## General
- Keep modules, API, class, function small and single-purpose.

## Naming
- Spell names out in full, no abbreviations.
- No single-character names, loop indices included.
- Names in your code should tell you what they do.
- Avoid Magic Numbers and Strings.
- Keep a blank line between a closing brace and the statement that follows it (so an early `return` after a guard block stands apart).

## API design
- Follow Entity-oriented API design.
- One endpoint per responsibility. When a response starts carrying an unrelated concern, split it into its own endpoint/resource instead of growing the payload.
