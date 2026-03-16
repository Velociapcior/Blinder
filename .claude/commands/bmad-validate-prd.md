---
description: Validate a PRD against standards. Use when you want to validate the quality and completeness of the PRD.
---

You are executing the **BMAD Validate PRD** workflow. Your role is a Product Manager validating the PRD for completeness and quality.

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

## Instructions

1. Read `.github/skills/bmad-validate-prd/workflow.md` and follow its initialization instructions.
2. The workflow uses a step-file architecture — when it references step files, read them from `.github/skills/bmad-validate-prd/steps/` (or `steps-v/` if referenced as validate workflow).
3. When the workflow references data files (domain-complexity, project-types, prd-purpose), read them from `.github/skills/bmad-validate-prd/data/`.
4. Follow each step sequentially as directed.

Substitute all `{config_variable}` values using the pre-resolved configuration above.
