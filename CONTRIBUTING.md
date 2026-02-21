# Contributing to Fox.RetryKit

Thank you for your interest in contributing to Fox.RetryKit! This document provides guidelines and instructions for contributing to the project.

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for all contributors.

## How to Contribute

### Reporting Issues

If you find a bug or have a feature request:

1. Check if the issue already exists in the [GitHub Issues](https://github.com/akikari/Fox.RetryKit/issues)
2. If not, create a new issue with:
   - Clear, descriptive title
   - Detailed description of the problem or feature
   - Steps to reproduce (for bugs)
   - Expected vs actual behavior
   - Code samples if applicable
   - Environment details (.NET version, OS, etc.)

### Submitting Changes

1. **Fork the repository** and create a new branch from `main`
2. **Make your changes** following the coding guidelines below
3. **Write or update tests** for your changes
4. **Update documentation** if needed (README, XML comments)
5. **Ensure all tests pass** (`dotnet test`)
6. **Ensure build succeeds** (`dotnet build`)
7. **Submit a pull request** with:
   - Clear description of changes
   - Reference to related issues
   - Summary of testing performed

## Coding Guidelines

Fox.RetryKit follows strict coding standards. Please review the [Copilot Instructions](.github/copilot-instructions.md) for detailed guidelines.

### Key Standards

#### General
- **Language**: All code, comments, and documentation must be in English
- **Line Endings**: CRLF
- **Indentation**: 4 spaces (no tabs)
- **Namespaces**: File-scoped (`namespace MyNamespace;`)
- **Nullable**: Enabled
- **Language Version**: latest

#### Naming Conventions
- **Private Fields**: camelCase without underscore prefix (e.g., `value`, not `_value`)
- **Public Members**: PascalCase
- **Local Variables**: camelCase

#### Code Style
- Use expression-bodied members for simple properties and methods
- Use auto-properties where possible
- Prefer `var` only when type is obvious
- Maximum line length: 100 characters
- Add blank line after closing brace UNLESS next line is also `}`

#### Documentation
- **XML Comments**: Required for all public APIs
- **Language**: English
- **Decorators**: 98 characters width using `//======` (no space after prefix)
- **File Headers**: 3-line header (purpose + technical description + decorators)

Example:
```csharp
//==================================================================================================
// Abstract base class for all retry policies with shared configuration and execution logic.
// Provides polymorphic factory methods for derived policy types with covariant return types.
//==================================================================================================

namespace Fox.RetryKit;

//==================================================================================================
/// <summary>
/// Abstract base class for retry policies that defines common behavior and configuration.
/// Derived classes implement specific retry strategies with covariant return types.
/// </summary>
//==================================================================================================
public abstract class RetryPolicyBase
{
    //==============================================================================================
    /// <summary>
    /// Gets the maximum number of retry attempts.
    /// </summary>
    //==============================================================================================
    public int RetryCount { get; }

    //==============================================================================================
    /// <summary>
    /// Configures a callback to be invoked before each retry attempt.
    /// </summary>
    /// <param name="callback">The callback action that receives exception, attempt number, and delay.</param>
    /// <returns>A new retry policy with the callback configured.</returns>
    //==============================================================================================
    public abstract RetryPolicyBase OnRetry(Action<Exception, int, TimeSpan> callback);
}
```

## Testing Requirements

- **Framework**: xUnit
- **Assertions**: FluentAssertions
- **Test Naming**: `MethodName_Should_ExpectedBehavior`
- **Coverage**: Aim for 100% coverage of new code
- **Test Structure**:
  - Arrange: Setup test data
  - Act: Execute the method under test
  - Assert: Verify expected behavior

Example:
```csharp
[Fact]
public void Retry_Should_CreatePolicyWithSpecifiedCount()
{
    // Arrange
    var retryCount = 3;

    // Act
    var policy = RetryPolicy.Retry(retryCount);

    // Assert
    policy.RetryCount.Should().Be(retryCount);
}

[Fact]
public void Execute_Should_RetryOnException()
{
    // Arrange
    var policy = RetryPolicy.Retry(3);
    var attempts = 0;

    // Act
    policy.Execute(() =>
    {
        attempts++;
        if (attempts < 3)
        {
            throw new InvalidOperationException("Test failure");
        }
    });

    // Assert
    attempts.Should().Be(3);
}
```

## Architecture Principles

Fox.RetryKit follows resilience engineering patterns and clean code practices:

- **Fluent API**: Chainable methods for policy configuration
- **Type Safety**: Compile-time safety with fluent builder pattern and covariant return types
- **Polymorphism**: Abstract base class (`RetryPolicyBase`) with derived policy types
- **Composability**: Combine retry, timeout, and exception filtering
- **Simplicity**: Minimal API surface, easy to understand
- **Zero Dependencies**: No external dependencies beyond base library

### Design Guidelines

- **Immutable Policies**: Fluent methods return new policy instances
- **Covariant Returns**: Override methods return specific derived types for type-safe chaining
- **Abstract Factory Methods**: Base class defines abstract `OnRetry()`, `WithJitter()`, `WithMaxDelay()` for polymorphic behavior
- **Design-Time Safety**: Type-specific methods (e.g., `Handle<T>()`) only available on appropriate policy types
- **Explicit Configuration**: RetryCount, Delay, Timeout are explicit properties
- **Exception Filtering**: Handle<T>() for selective retry
- **Async-First**: Full async/await support with CancellationToken
- **Telemetry-Friendly**: ExecuteWithResult for observability

## Project Structure

```
Fox.RetryKit/
├── src/
│   └── Fox.RetryKit/              # Core package
│       ├── RetryPolicyBase.cs     # Abstract base class for all policies
│       ├── RetryPolicy.cs         # Main policy class
│       ├── CustomDelayRetryPolicy.cs # Custom delay sequences
│       ├── RetryIfPolicy.cs       # Conditional retry
│       ├── RetryResult.cs         # Telemetry results
│       ├── RetryExecutor.cs       # Internal executor
│       ├── DelayStrategy.cs       # Delay strategies enum
│       ├── RetryPolicyExtensions.cs # Extension methods
│       ├── GlobalSuppressions.cs  # Code analysis suppressions
│       └── README.md              # Package documentation
├── tests/
│   └── Fox.RetryKit.Tests/        # Unit tests
│       ├── RetryTests.cs          # Basic retry tests
│       ├── DelayTests.cs          # Delay strategy tests
│       ├── TimeoutTests.cs        # Timeout tests
│       ├── ExceptionFilterTests.cs # Exception filtering tests
│       ├── OnRetryTests.cs        # Callback tests
│       ├── JitterTests.cs         # Jitter tests
│       ├── MaxDelayTests.cs       # MaxDelay tests
│       ├── WaitAndRetryTests.cs   # Custom delay tests
│       ├── FallbackTests.cs       # Fallback tests
│       ├── RetryIfTests.cs        # Conditional retry tests
│       └── RetryResultTests.cs    # Telemetry tests
├── samples/
│   └── Fox.RetryKit.Demo/         # Demo application
│       └── Program.cs             # Usage examples (14 examples)
└── assets/
    └── icon.png                   # Package icon
```

## Pull Request Process

1. **Update tests**: Ensure your changes are covered by tests
2. **Update documentation**: Keep README and XML comments up to date
3. **Follow coding standards**: Use provided `.editorconfig` and copilot instructions
4. **Keep commits clean**: 
   - Use clear, descriptive commit messages
   - Squash commits if needed before merging
5. **Update CHANGELOG.md**: Add entry under `[Unreleased]` section
6. **Ensure CI passes**: All tests must pass and build must succeed

### Commit Message Format

Use clear, imperative commit messages:

```
Add OnRetry callback for observability

- Implement OnRetry(Action<Exception, int, TimeSpan>) method
- Add unit tests for OnRetry callback
- Update documentation and examples
```

## Feature Requests

When proposing new features, please consider:

1. **Scope**: Does this fit the minimalist nature of Fox.RetryKit?
2. **Complexity**: Does this add unnecessary complexity?
3. **Dependencies**: Does this require new external dependencies? (Should be avoided)
4. **Breaking Changes**: Will this break existing code?
5. **Use Cases**: What real-world scenarios does this address?
6. **Resilience Patterns**: Does this align with established resilience patterns?

Fox.RetryKit aims to be lightweight, dependency-free, and focused on retry and timeout patterns. Features should align with resilience engineering principles and provide practical value for handling transient failures.

## Development Setup

### Prerequisites
- .NET 8 SDK or later
- Visual Studio 2022+ or Rider (recommended)
- Git

### Getting Started

1. Clone the repository:
```bash
git clone https://github.com/akikari/Fox.RetryKit.git
cd Fox.RetryKit
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the solution:
```bash
dotnet build
```

4. Run tests:
```bash
dotnet test
```

5. Run the demo application:
```bash
dotnet run --project samples/Fox.RetryKit.Demo/Fox.RetryKit.Demo.csproj
```

6. Create NuGet package:
```bash
dotnet pack src/Fox.RetryKit/Fox.RetryKit.csproj -c Release
```

## Questions?

If you have questions about contributing, feel free to:
- Open a [GitHub Discussion](https://github.com/akikari/Fox.RetryKit/discussions)
- Create an issue labeled `question`
- Reach out to the maintainers

## License

By contributing to Fox.RetryKit, you agree that your contributions will be licensed under the MIT License.

Thank you for contributing to Fox.RetryKit! 🎉
