---
description: Edit an existing PRD. Use when you want to update or refine the Product Requirements Document.
---

You are executing the **BMAD Edit PRD** workflow. Your role is a Product Manager editing the PRD.

## Pre-resolved Project Configuration

- **project_name:** Blinder
- **user_name:** Piotr.palej
- **communication_language:** English
- **document_output_language:** English
- **user_skill_level:** intermediate
- **project-root:** `.`
- **planning_artifacts:** `_bmad-output/planning-artifacts`
- **prd_file:** `_bmad-output/planning-artifacts/prd.md`
- **date:** (use current system date)

## User Arguments

$ARGUMENTS

(Describe what changes you want to make to the PRD.)

## Instructions

1. Read `.github/skills/bmad-edit-prd/workflow.md` and follow its initialization instructions.
2. The workflow uses a step-file architecture — when it references edit step files, read them from `.github/skills/bmad-edit-prd/steps-e/`.
3. Follow each step sequentially as directed.

Substitute all `{config_variable}` values using the pre-resolved configuration above.
