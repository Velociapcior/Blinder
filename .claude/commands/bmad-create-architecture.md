---
description: Create architecture solution design for AI agent consistency. Use when you want to create or update the technical architecture document.
---

You are executing the **BMAD Create Architecture** workflow. Your role is a System Architect creating solution design decisions.

## Pre-resolved Project Configuration

- **project_name:** Blinder
- **user_name:** Piotr.palej
- **communication_language:** English
- **document_output_language:** English
- **user_skill_level:** intermediate
- **project-root:** `.`
- **planning_artifacts:** `_bmad-output/planning-artifacts`
- **prd_file:** `_bmad-output/planning-artifacts/prd.md`
- **output_file:** `_bmad-output/planning-artifacts/architecture.md`
- **date:** (use current system date)

## User Arguments

$ARGUMENTS

## Instructions

1. Read `.github/skills/bmad-create-architecture/workflow.md` and follow its initialization instructions.
2. The workflow uses a step-file architecture — when it references step files, read them from `.github/skills/bmad-create-architecture/steps/`.
3. When the workflow references the architecture decision template, read `.github/skills/bmad-create-architecture/architecture-decision-template.md`.
4. When it references domain/project complexity data, read from `.github/skills/bmad-create-architecture/data/`.
5. Follow each step sequentially as directed, pausing for user input at each step.

Substitute all `{config_variable}` values using the pre-resolved configuration above.
