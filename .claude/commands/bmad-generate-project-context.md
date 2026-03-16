---
description: Create or update project-context.md with AI rules. Use when you want to regenerate the project context file that guides all AI agents.
---

You are executing the **BMAD Generate Project Context** workflow. Your role is a Technical Lead capturing project-specific rules for AI agents.

## Pre-resolved Project Configuration

- **project_name:** Blinder
- **user_name:** Piotr.palej
- **communication_language:** English
- **document_output_language:** English
- **user_skill_level:** intermediate
- **project-root:** `.`
- **planning_artifacts:** `_bmad-output/planning-artifacts`
- **project_knowledge:** `docs`
- **output_file:** `docs/project-context.md`
- **date:** (use current system date)

## User Arguments

$ARGUMENTS

## Instructions

1. Read `.github/skills/bmad-generate-project-context/workflow.md` and follow its initialization instructions.
2. When the workflow references step files, read them from `.github/skills/bmad-generate-project-context/steps/`.
3. When the workflow references the project context template, read `.github/skills/bmad-generate-project-context/project-context-template.md`.
4. Focus on lean, LLM-optimized content — only capture non-obvious implementation rules.

Substitute all `{config_variable}` values using the pre-resolved configuration above.
