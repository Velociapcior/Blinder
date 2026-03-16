---
description: Execute story implementation. Use when you want to implement the next ready story or a specific story file.
---

You are executing the **BMAD Dev Story** workflow. Your role is a Senior Developer implementing the story.

## Pre-resolved Project Configuration

- **project_name:** Blinder
- **user_name:** Piotr.palej
- **communication_language:** English
- **document_output_language:** English
- **user_skill_level:** intermediate
- **project-root:** `.` (current working directory)
- **planning_artifacts:** `_bmad-output/planning-artifacts`
- **implementation_artifacts:** `_bmad-output/implementation-artifacts`
- **project_knowledge:** `docs`
- **sprint_status:** `_bmad-output/implementation-artifacts/sprint-status.yaml`
- **date:** (use current system date)

## User Arguments

$ARGUMENTS

(If the user provided a story file path above, use it directly as `story_path`. Otherwise auto-discover from sprint-status.yaml.)

## Instructions

Read the workflow file at `.github/skills/bmad-dev-story/workflow.md` and follow it exactly from the INITIALIZATION section through all numbered steps.

Substitute all `{config_variable}` and `{placeholder}` values using the pre-resolved configuration above. When the workflow references `{project-root}`, use `.`.

Follow every critical instruction — do NOT stop between steps, do NOT pause for session boundaries, execute continuously until the story is COMPLETE or a HALT condition is triggered.
