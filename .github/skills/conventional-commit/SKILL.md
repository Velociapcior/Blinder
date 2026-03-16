---
name: conventional-commit
description: 'Prompt and workflow for generating conventional commit messages using a structured XML format. Guides users to create standardized, descriptive commit messages in line with the Conventional Commits specification, including instructions, examples, and validation.'
---

# Conventional Commit

This skill provides a workflow for generating conventional commit messages. It provides instructions, examples, and formatting guidelines to help write standardized, descriptive commit messages in accordance with the [Conventional Commits specification](https://www.conventionalcommits.org/en/v1.0.0/).

## Workflow

**Follow these steps:**

1. Run `git status` to review changed files.
2. Run `git diff` or `git diff --cached` to inspect changes.
3. Stage your changes with `git add <file>`.
4. Construct your commit message using the following structure.
5. After generating your commit message, Copilot will automatically run the commit command in the terminal.

## Commit Message Structure

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

Where:
- **type**: `feat|fix|docs|style|refactor|perf|test|build|ci|chore|revert`
- **scope**: optional context (e.g., `auth`, `matching`, `photos`, `api`, `mobile`)
- **description**: a short, imperative summary of the change
- **body**: optional more detailed explanation
- **footer**: optional, e.g., `BREAKING CHANGE: details` or issue references

## Examples

```
feat(auth): add Apple Sign-In support
fix(matching): correct score calculation for distance weighting
docs: update deployment instructions for Hetzner VPS
refactor(photos): extract presigned URL generation to helper method
perf(matching): cache compatibility scores for 5 minutes
test(auth): add unit tests for JWT refresh flow
chore: update NuGet packages to latest stable
ci: add Docker image build workflow
feat!: change photo reveal API response shape (BREAKING CHANGE: clients must update)
```

## Validation Rules

- **type**: Must be one of the allowed types listed above
- **scope**: Optional, but recommended for clarity in a multi-domain codebase
- **description**: Required. Use the imperative mood (e.g., "add", not "added")
- **body**: Optional. Use for additional context or motivation
- **footer**: Use for breaking changes (`BREAKING CHANGE:`) or issue references (`Closes #123`)

## Final Step

```bash
git commit -m "type(scope): description"
# Include -m again for body if needed:
git commit -m "type(scope): description" -m "Longer explanation of context."
```
