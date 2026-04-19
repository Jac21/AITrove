# Copilot instructions — .NET / C# project

<!-- 
  Place this file at: .github/copilot-instructions.md
  GitHub Copilot reads it automatically in VS Code and Visual Studio.
  Customize the sections marked [YOUR PROJECT] before committing.
-->

## Project overview

[YOUR PROJECT] is a [brief description] built with .NET 9 and C#.

## Stack & conventions

- **Runtime**: .NET 9, C# 13, nullable reference types enabled
- **DI**: `Microsoft.Extensions.DependencyInjection` — register services in `Program.cs` or `Startup.cs`
- **Async**: All I/O must be `async/await`. Always propagate `CancellationToken` through call chains.
- **HTTP**: Never instantiate `HttpClient` directly in hot paths — use `IHttpClientFactory` or inject a shared instance.
- **Logging**: Inject `ILogger<T>`. Use structured logging with message templates, not string interpolation.
- **Error handling**: Throw specific exceptions. Catch at boundaries, not in every method.
- **Records vs classes**: Prefer `record` for immutable data transfer objects.

## Code style

- Follow the [Microsoft C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- `var` is preferred when the type is obvious from the right-hand side
- Primary constructors preferred for simple DI injection
- File-scoped namespaces always
- No `#region` blocks

## What to avoid

- ❌ `new HttpClient()` inside methods — HttpClient should be shared
- ❌ `.Result` or `.Wait()` on async code — always `await`
- ❌ Catching `Exception` at low levels — let it bubble
- ❌ Magic strings for configuration keys — use strongly-typed options
- ❌ Business logic in controllers or minimal API handlers

## Testing conventions

- xUnit for unit tests, FluentAssertions for assertions
- Test class naming: `{SystemUnderTest}Tests`
- Test method naming: `{Method}_{Scenario}_{ExpectedResult}`
- Mock boundaries (HTTP, DB) with `NSubstitute` or `Moq`

## Project structure

```
src/
  [YourProject]/          ← main project
  [YourProject].Tests/    ← unit tests
```
