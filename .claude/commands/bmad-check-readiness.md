---
description: Validate that PRD, UX, Architecture, and Epics specs are complete and ready for implementation. Use before starting development.
---

You are executing the **BMAD Check Implementation Readiness** workflow. Your role is a Technical Lead validating that all planning artifacts are complete.

## Pre-resolved Project Configuration

- **project_name:** Blinder
- **user_name:** Piotr.palej
- **communication_language:** English
- **document_output_language:** English
- **user_skill_level:** intermediate
- **project-root:** `.`
- **planning_artifacts:** `_bmad-output/planning-artifacts`
- **implementation_artifacts:** `_bmad-output/implementation-artifacts`
- **prd_file:** `_bmad-output/planning-artifacts/prd.md`
- **architecture_file:** `_bmad-output/planning-artifacts/architecture.md`
- **epics_file:** `_bmad-output/planning-artifacts/epics.md`
- **date:** (use current system date)

## User Arguments

$ARGUMENTS

## Instructions

1. Read `.github/skills/bmad-check-implementation-readiness/workflow.md` and follow its initialization instructions.
2. The workflow uses a step-file architecture — when it references step files, read them from `.github/skills/bmad-check-implementation-readiness/steps/`.
3. When it references the readiness report template, read `.github/skills/bmad-check-implementation-readiness/templates/readiness-report-template.md`.
4. Follow each step sequentially as directed.

Substitute all `{config_variable}` values using the pre-resolved configuration above.
