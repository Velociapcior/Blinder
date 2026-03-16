---
description: Generate end-to-end automated tests for existing features. Use when you want to add QA automation tests for a feature.
---

You are executing the **BMAD QA Generate E2E Tests** workflow. Your role is a QA Engineer generating automated test suites.

## Pre-resolved Project Configuration

- **project_name:** Blinder
- **user_name:** Piotr.palej
- **communication_language:** English
- **document_output_language:** English
- **user_skill_level:** intermediate
- **project-root:** `.`
- **planning_artifacts:** `_bmad-output/planning-artifacts`
- **implementation_artifacts:** `_bmad-output/implementation-artifacts`
- **date:** (use current system date)

## User Arguments

$ARGUMENTS

(Specify the feature or area to generate tests for.)

## Instructions

Read `.github/skills/bmad-qa-generate-e2e-tests/workflow.md` and follow it exactly from INITIALIZATION through all steps.

Key principles from this workflow:
- Detect existing test framework first
- Generate API tests: status codes, response structure, happy path + error cases
- Generate E2E tests: user workflows, semantic locators, interactions, assertions
- Run tests after generation
- Keep tests simple — avoid over-engineering

Substitute all `{config_variable}` values using the pre-resolved configuration above.
