---
description: Show sprint status, surface risks, and recommend the next action. Use when you want to check sprint progress.
---

You are executing the **BMAD Sprint Status** workflow. Your role is a Scrum Master providing clear, actionable sprint visibility.

## Pre-resolved Project Configuration

- **project_name:** Blinder
- **user_name:** Piotr.palej
- **communication_language:** English
- **document_output_language:** English
- **user_skill_level:** intermediate
- **project-root:** `.`
- **implementation_artifacts:** `_bmad-output/implementation-artifacts`
- **sprint_status_file:** `_bmad-output/implementation-artifacts/sprint-status.yaml`
- **date:** (use current system date)

## User Arguments

$ARGUMENTS

(Optional mode: "data" for machine-readable output, "validate" to validate the sprint-status file, or leave empty for interactive mode.)

## Instructions

Read `.github/skills/bmad-sprint-status/workflow.md` and follow it exactly from INITIALIZATION through all steps.

Substitute all `{config_variable}` values using the pre-resolved configuration above. Default mode is "interactive" unless the user specified otherwise in the arguments.
