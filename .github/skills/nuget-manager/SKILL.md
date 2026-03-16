---
name: nuget-manager
description: 'Manage NuGet packages in .NET projects/solutions. Use this skill when adding, removing, or updating NuGet package versions. It enforces using `dotnet` CLI for package management and provides strict procedures for direct file edits only when updating versions.'
---

# NuGet Manager

## Overview

This skill ensures consistent and safe management of NuGet packages across .NET projects. It prioritizes using the `dotnet` CLI to maintain project integrity and enforces a strict verification and restoration workflow for version updates.

## Prerequisites

- .NET SDK installed (compatible with the target solution)
- `dotnet` CLI available on your `PATH`
- PowerShell (for version verification using `dotnet package search`)

## Core Rules

1. **NEVER** directly edit `.csproj`, `.props`, or `Directory.Packages.props` files to **add** or **remove** packages. Always use `dotnet add package` and `dotnet remove package` commands.
2. **DIRECT EDITING** is ONLY permitted for **changing versions** of existing packages.
3. **VERSION UPDATES** must follow the mandatory workflow:
   - Verify the target version exists on NuGet.
   - Determine if versions are managed per-project (`.csproj`) or centrally (`Directory.Packages.props`).
   - Update the version string in the appropriate file.
   - Immediately run `dotnet restore` to verify compatibility.

## Workflows

### Adding a Package
Use `dotnet add [<PROJECT>] package <PACKAGE_NAME> [--version <VERSION>]`.

```bash
# Example
dotnet add backend/Blinder.Api/Blinder.Api.csproj package Serilog.AspNetCore
```

### Removing a Package
Use `dotnet remove [<PROJECT>] package <PACKAGE_NAME>`.

```bash
# Example
dotnet remove backend/Blinder.Api/Blinder.Api.csproj package SomeOldPackage
```

### Updating Package Versions

When updating a version, follow these steps:

1. **Verify Version Existence**:
   Check if the version exists using the `dotnet package search` command with exact match and JSON formatting.

   Using PowerShell:
   ```powershell
   (dotnet package search <PACKAGE_NAME> --exact-match --format json | ConvertFrom-Json).searchResult.packages | Where-Object { $_.version -eq "<VERSION>" }
   ```

2. **Determine Version Management**:
   - Search for `Directory.Packages.props` in the solution root. If present, versions should be managed there via `<PackageVersion Include="Package.Name" Version="1.2.3" />`.
   - If absent, check individual `.csproj` files for `<PackageReference Include="Package.Name" Version="1.2.3" />`.

3. **Apply Changes**:
   Modify the identified file with the new version string.

4. **Verify Stability**:
   Run `dotnet restore` on the project or solution. If errors occur, revert the change and investigate.

## Examples

### "Add FluentValidation to the API project"
```bash
dotnet add backend/Blinder.Api/Blinder.Api.csproj package FluentValidation.AspNetCore
```

### "Update Serilog to 4.0.0 in the solution"
1. Verify 4.0.0 exists: `dotnet package search Serilog --exact-match --format json`
2. Find where it's defined (e.g., `Directory.Packages.props` or `Blinder.Api.csproj`)
3. Edit the file to update the version string
4. Run `dotnet restore backend/Blinder.sln`
