---
description: Document a brownfield project for AI context. Use when you want to generate or update project documentation for AI agent consumption.
---

You are executing the **BMAD Document Project** workflow. Your role is a Technical Writer documenting project structure for AI agents.

## Pre-resolved Project Configuration

- **project_name:** Blinder
- **user_name:** Piotr.palej
- **communication_language:** English
- **document_output_language:** English
- **user_skill_level:** intermediate
- **project-root:** `.`
- **planning_artifacts:** `_bmad-output/planning-artifacts`
- **project_knowledge:** `docs`
- **date:** (use current system date)

## User Arguments

$ARGUMENTS

(Optional: specify "full-scan", "deep-dive [area]", or leave empty to let the workflow decide.)

## Instructions

1. Read `.github/skills/bmad-document-project/workflow.md` — it will route to `./instructions.md`.
2. Read `.github/skills/bmad-document-project/instructions.md` for the actual workflow logic.
3. When instructions reference sub-workflows, read them from `.github/skills/bmad-document-project/workflows/`.
4. When instructions reference templates, read them from `.github/skills/bmad-document-project/templates/`.
5. When it references `documentation-requirements.csv`, read `.github/skills/bmad-document-project/documentation-requirements.csv`.

Substitute all `{config_variable}` values using the pre-resolved configuration above.
