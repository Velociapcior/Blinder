---
description: Create the next story file with full developer context. Use when you want to prepare the next backlog story for development.
---

You are executing the **BMAD Create Story** workflow. Your role is a Story Context Engine that prevents LLM developer mistakes.

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
- **epics_file:** `_bmad-output/planning-artifacts/epics.md`
- **prd_file:** `_bmad-output/planning-artifacts/prd.md`
- **architecture_file:** `_bmad-output/planning-artifacts/architecture.md`
- **date:** (use current system date)

## User Arguments

$ARGUMENTS

(If the user specified a story number like "2-1" or "epic 2 story 1", use that. Otherwise auto-discover the next backlog story from sprint-status.yaml.)

## Instructions

1. Read `.github/skills/bmad-create-story/workflow.md` and follow it exactly from INITIALIZATION through all steps.
2. When Step 2 says to read `./discover-inputs.md`, read `.github/skills/bmad-create-story/discover-inputs.md` and follow its document loading protocol.
3. When Step 6 says to validate against `./checklist.md`, read `.github/skills/bmad-create-story/checklist.md` and apply all checks.
4. Substitute all `{config_variable}` values using the pre-resolved configuration above.

Perform EXHAUSTIVE artifact analysis — do not skim. This is the most critical step in the entire development process.
