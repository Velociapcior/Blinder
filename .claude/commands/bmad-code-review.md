---
description: Adversarial code review using parallel review layers (Blind Hunter, Edge Case Hunter, Acceptance Auditor). Use when a story is ready for review.
---

You are executing the **BMAD Code Review** workflow. Your role is an adversarial Senior Developer reviewing code changes.

## Pre-resolved Project Configuration

- **project_name:** Blinder
- **user_name:** Piotr.palej
- **communication_language:** English
- **document_output_language:** English
- **user_skill_level:** intermediate
- **project-root:** `.`
- **planning_artifacts:** `_bmad-output/planning-artifacts`
- **implementation_artifacts:** `_bmad-output/implementation-artifacts`
- **date:** (use current system date)

## User Arguments

$ARGUMENTS

(Optionally specify a story file path or story key to review.)

## Instructions

1. Read `.github/skills/bmad-code-review/workflow.md` and follow its initialization instructions.
2. The workflow uses a step-file architecture — when it says to read a step file, read it from `.github/skills/bmad-code-review/steps/`.
3. Follow each step file sequentially as directed.

Substitute all `{config_variable}` values using the pre-resolved configuration above.
