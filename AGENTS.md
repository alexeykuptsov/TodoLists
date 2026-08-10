You are an AI engineer, expert in writing prompts.

Read the project summary from `README.md`.

Read technical specification from `TECHNICAL_SPECIFICATION.md`.

# Task tracking system

This project uses a lightweight file-based task tracker under `Tasks/`.

- `Tasks/INDEX.md` is the index file for the list of tasks. It contains one markdown table with columns `ID | Title | Status | Link`. `Status` is one of `Todo`, `In Progress`, `Done`. Tasks are added at the bottom in numeric order (`T-1`, `T-2`, ...).
- Each task lives in its own file `Tasks/T-<N>_<Short_Title>.md` using this exact structure and headings, in this order: `# T-<N>: <Title>`, `## Summary`, `## Context / Background`, `## Acceptance Criteria` (as a `- [ ]` checklist), `## Implementation Notes / Decisions`, then a `---` separator, then `## Prompt (paste into a clean Claude context)` followed by exactly one fenced code block. Nothing comes after that fenced block.
- The fenced "Prompt" block must be **fully self-contained** — a fresh Claude session with no other context (not even the task file above it) must be able to implement the task from just the pasted block. Repeat the essential context, decisions, and acceptance criteria inline. Use a four-backtick outer fence so inner triple-backtick code blocks stay valid.
- Cross-reference related tasks with `[[T-N]]` links inside task bodies.

When the user asks to add a task: create the task file, add a row to `INDEX.md`, and compose the Prompt block last (after the AC and decisions have settled) so it stays in sync. When the user asks to work on a task, read its file first — the Implementation Notes usually capture decisions that are not yet in the code. When a task is finished or dropped, update its `Status` in `INDEX.md` (don't delete the file — the prompt and decisions are still useful history).

# Rules to follow in your work

## Use PowerShell for small shell scripts

When running console commands use PowerShell syntax.
Do not use `&&` and other Bash syntax and commands.

## Prefer C# one file programs for small temporary scripts

Plase such scripts in directory `Tmp`.
