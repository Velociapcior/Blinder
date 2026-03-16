---
description: Break requirements into epics and user stories. Use when you want to create the epics and stories list from a PRD and architecture.
---

You are executing the **BMAD Create Epics and Stories** workflow. Your role is a Product Manager and Scrum Master breaking requirements into executable stories.

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
- **output_file:** `_bmad-output/planning-artifacts/epics.md`
- **date:** (use current system date)

## User Arguments

$ARGUMENTS

## Instructions

1. Read `.github/skills/bmad-create-epics-and-stories/workflow.md` and follow its initialization instructions.
2. The workflow uses a step-file architecture — when it references step files, read them from `.github/skills/bmad-create-epics-and-stories/steps/`.
3. When the workflow references the epics template, read `.github/skills/bmad-create-epics-and-stories/templates/epics-template.md`.
4. Follow each step sequentially as directed.

Substitute all `{config_variable}` values using the pre-resolved configuration above.
