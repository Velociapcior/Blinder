---
name: dotnet-best-practices
description: 'Ensure .NET/C# code meets best practices for the solution/project.'
---

# .NET/C# Best Practices

Your task is to ensure .NET/C# code in ${selection} meets the best practices specific to this solution/project. This includes:

## Documentation & Structure

- Create comprehensive XML documentation comments for all public classes, interfaces, methods, and properties
- Include parameter descriptions and return value descriptions in XML comments
- Follow the established namespace structure: `{Core|Console|App|Service}.{Feature}`

## Research Approach

- Never use reflection-based inspection, assembly loading, or local package-cache probing to discover .NET/backend framework APIs.
- When verifying framework/package behavior for backend development, use official web documentation first.
- Prefer Microsoft Learn, official vendor documentation, GitHub documentation, and release notes over local binary inspection.

## Design Patterns & Architecture

- Use primary constructor syntax for dependency injection (e.g., `public class MyClass(IDependency dependency)`)
- Implement the Command Handler pattern with generic base classes (e.g., `CommandHandler<TOptions>`)
- Use interface segregation with clear naming conventions (prefix interfaces with 'I')
- Follow the Factory pattern for complex object creation

## Dependency Injection & Services

- Use constructor dependency injection with null checks via `ArgumentNullException`
- Register services with appropriate lifetimes (Singleton, Scoped, Transient)
- Use `Microsoft.Extensions.DependencyInjection` patterns
- Implement service interfaces for testability

## Async/Await Patterns

- Use async/await for all I/O operations and long-running tasks
- Return `Task` or `Task<T>` from async methods
- Use `ConfigureAwait(false)` where appropriate
- Handle async exceptions properly

## Testing Standards

- Use xUnit framework with FluentAssertions for assertions
- Follow AAA pattern (Arrange, Act, Assert)
- Use NSubstitute or Moq for mocking dependencies
- Test both success and failure scenarios
- Include null parameter validation tests

## Configuration & Settings

- Use strongly-typed configuration classes with data annotations
- Implement validation attributes (`Required`, `NotEmptyOrWhitespace`)
- Use `IConfiguration` binding for settings
- Support `appsettings.json` configuration files

## Error Handling & Logging

- Use structured logging with `Microsoft.Extensions.Logging` / Serilog
- Include scoped logging with meaningful context
- Throw specific exceptions with descriptive messages
- Use RFC 7807 Problem Details for API error responses

## Performance & Security

- Use C# 14 / .NET 10 features where applicable
- Implement proper input validation and sanitization
- Use parameterized queries for database operations
- Follow secure coding practices

## Code Quality

- Ensure SOLID principles compliance
- Avoid code duplication through base classes and utilities
- Use meaningful names that reflect domain concepts
- Keep methods focused and cohesive
- Implement proper disposal patterns for resources
