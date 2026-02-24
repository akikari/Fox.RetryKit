# Fox.RetryKit

[![.NET](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-512BD4)](https://dotnet.microsoft.com/)
[![Build and Test](https://img.shields.io/github/actions/workflow/status/akikari/Fox.RetryKit/build-and-test.yml?branch=main&label=build%20and%20test&color=darkgreen)](https://github.com/akikari/Fox.RetryKit/actions/workflows/build-and-test.yml)
[![NuGet](https://img.shields.io/nuget/v/Fox.RetryKit.svg)](https://www.nuget.org/packages/Fox.RetryKit/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Fox.RetryKit?label=downloads&color=darkgreen)](https://www.nuget.org/packages/Fox.RetryKit/)
[![License: MIT](https://img.shields.io/badge/license-MIT-orange.svg)](https://opensource.org/licenses/MIT)
[![codecov](https://img.shields.io/codecov/c/github/akikari/Fox.RetryKit?color=darkgreen&label=codecov)](https://codecov.io/gh/akikari/Fox.RetryKit)

A minimalist, dependency-free retry and timeout utility for C#. Build resilient applications with simple, type-safe retry policies.

## Why Fox.RetryKit?

Fox.RetryKit provides a clean, lightweight implementation of retry and timeout patterns without any dependencies:

- **Zero Dependencies** - No external dependencies, not even Microsoft.Extensions.*
- **Type-Safe** - Fluent API with compile-time safety
- **Flexible** - Fixed delays, exponential backoff, and custom delay sequences
- **Timeout Support** - Built-in timeout with cancellation token integration
- **Exception Filtering** - Retry only specific exception types
- **Async-First** - Full support for async/await patterns
- **Observability** - OnRetry callbacks for logging and metrics
- **Jitter** - Random variation to prevent thundering herd
- **Conditional Retry** - Retry based on result values (RetryIf)
- **Fallback** - Graceful degradation with default values
- **Telemetry** - Detailed retry metrics (ExecuteWithResult)
- **Lightweight** - Minimal overhead, simple API, fast compilation
- **Well-Documented** - Comprehensive XML documentation for IntelliSense

## Installation

```bash
dotnet add package Fox.RetryKit
```

## Quick Start

### 1. Basic Retry

```csharp
using Fox.RetryKit;

// Retry up to 3 times
var policy = RetryPolicy.Retry(3);

policy.Execute(() =>
{
    // Your potentially failing operation
    CallUnstableService();
});
```

### 2. Retry with Delay

```csharp
// Retry 3 times with 500ms delay between attempts
var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(500));

await policy.ExecuteAsync(async () =>
{
    await CallUnstableApiAsync();
});
```

### 3. Exponential Backoff

```csharp
// Retry 5 times with exponential backoff (100ms, 200ms, 400ms, 800ms, 1600ms)
var policy = RetryPolicy.ExponentialBackoff(5, TimeSpan.FromMilliseconds(100));

var result = await policy.ExecuteAsync(async () =>
{
    return await FetchDataAsync();
});
```

### 4. Timeout

```csharp
// Cancel operation if it takes longer than 5 seconds
var policy = RetryPolicy.Timeout(TimeSpan.FromSeconds(5));

await policy.ExecuteAsync(async () =>
{
    await LongRunningOperationAsync();
});
```

### 5. Exception Filtering

```csharp
// Retry only on specific exception types
var policy = RetryPolicy.Retry(3)
    .Handle<HttpRequestException>()
    .Handle<TimeoutException>();

await policy.ExecuteAsync(async () =>
{
    await CallExternalApiAsync();
});
```

### 6. Combined Policies

```csharp
// Combine retry, delay, timeout, and exception filtering
var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(500))
    .Handle<HttpRequestException>()
    .WithTimeout(TimeSpan.FromSeconds(10));

var data = await policy.ExecuteAsync(async () =>
{
    return await FetchFromUnstableServiceAsync();
});
```

## Extended Features

### 7. OnRetry Callback (Observability)

Add logging or metrics on each retry attempt:

```csharp
var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(100))
    .OnRetry((exception, attempt, delay) =>
    {
        _logger.LogWarning("Retry attempt {Attempt} after {Delay}ms: {Message}",
            attempt, delay.TotalMilliseconds, exception.Message);
    });

await policy.ExecuteAsync(async () => await CallServiceAsync());
```

### 8. Jitter (Prevent Thundering Herd)

Add random variation (±25%) to delays to prevent synchronized retries:

```csharp
var policy = RetryPolicy.Retry(5, TimeSpan.FromMilliseconds(200))
    .WithJitter(); // Delays will vary: 150-250ms randomly

await policy.ExecuteAsync(async () => await CallSharedResourceAsync());
```

### 9. MaxDelay Cap

Limit exponential backoff growth to prevent excessive wait times:

```csharp
var policy = RetryPolicy.ExponentialBackoff(10, TimeSpan.FromMilliseconds(100))
    .WithMaxDelay(TimeSpan.FromSeconds(5)); // Cap at 5 seconds

await policy.ExecuteAsync(async () => await CallServiceAsync());
```

### 10. WaitAndRetry (Custom Delay Sequence)

Define a custom progression of delays:

```csharp
var delays = new[]
{
    TimeSpan.FromMilliseconds(100),
    TimeSpan.FromMilliseconds(250),
    TimeSpan.FromMilliseconds(500),
    TimeSpan.FromSeconds(1),
    TimeSpan.FromSeconds(2)
};

var policy = RetryPolicy.WaitAndRetry(delays);

await policy.ExecuteAsync(async () => await CallServiceAsync());
```

### 11. Fallback (Graceful Degradation)

Provide a default value if all retries fail:

```csharp
var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(100));

// Static fallback value
var result = policy.Fallback(() => GetRemoteData(), "default-value");

// Dynamic fallback provider
var result = policy.Fallback(
    () => GetPrimaryData(),
    () => GetCachedData()
);

// Async versions
var result = await policy.FallbackAsync(
    async () => await GetRemoteDataAsync(),
    "default-value"
);
```

### 12. RetryIf (Conditional Retry)

Retry based on the result value, not just exceptions:

```csharp
var policy = RetryPolicy.Retry(5, TimeSpan.FromMilliseconds(100))
    .RetryIf<HttpResponseMessage>(response => !response.IsSuccessStatusCode);

var response = await policy.ExecuteAsync(async () =>
{
    return await httpClient.GetAsync(url);
});
```

### 13. RetryResult (Telemetry)

Get detailed metrics about retry operations:

```csharp
var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(150));

var result = await policy.ExecuteWithResultAsync(async () =>
{
    return await FetchDataAsync();
});

Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Value: {result.Value}");
Console.WriteLine($"Attempts: {result.Attempts}");
Console.WriteLine($"Duration: {result.TotalDuration}");
Console.WriteLine($"Last Exception: {result.LastException?.Message}");
```

## Real-World Examples

### HTTP API Calls with Retry

```csharp
var httpPolicy = RetryPolicy.Retry(5, TimeSpan.FromMilliseconds(500))
    .Handle<HttpRequestException>()
    .WithTimeout(TimeSpan.FromSeconds(30));

var response = await httpPolicy.ExecuteAsync(async () =>
{
    using var client = new HttpClient();
    return await client.GetStringAsync("https://api.example.com/data");
});
```

### Database Operations

```csharp
var dbPolicy = RetryPolicy.ExponentialBackoff(3, TimeSpan.FromSeconds(1))
    .Handle<SqlException>()
    .Handle<TimeoutException>();

await dbPolicy.ExecuteAsync(async () =>
{
    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    // Execute queries
});
```

### File I/O with Retry

```csharp
var filePolicy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(100))
    .Handle<IOException>();

var content = filePolicy.Execute(() =>
{
    return File.ReadAllText("config.json");
});
```

## Architecture

Fox.RetryKit is built around a simple, composable `RetryPolicy` class:

```
RetryPolicy
├── Retry(count)                     → Immediate retry
├── Retry(count, delay)              → Fixed delay retry
├── ExponentialBackoff(count, delay) → Exponential backoff retry
└── Timeout(duration)                → Timeout constraint

Fluent Extensions
├── Handle<TException>()             → Exception filtering
└── WithTimeout(duration)            → Add timeout to retry policy

Execution
├── Execute(action)                  → Synchronous execution
├── Execute<T>(func)                 → Synchronous with result
├── ExecuteAsync(func)               → Asynchronous execution
└── ExecuteAsync<T>(func)            → Asynchronous with result
```

## API Reference

### RetryPolicy Static Methods

```csharp
// Create retry policy with count
RetryPolicy.Retry(int count)

// Create retry policy with count and fixed delay
RetryPolicy.Retry(int count, TimeSpan delay)

// Create exponential backoff policy
RetryPolicy.ExponentialBackoff(int retries, TimeSpan initialDelay)

// Create timeout policy
RetryPolicy.Timeout(TimeSpan duration)

// Create custom delay sequence policy
RetryPolicy.WaitAndRetry(IEnumerable<TimeSpan> delays)
```

### Instance Methods

```csharp
// Filter exceptions to retry
policy.Handle<TException>()

// Add timeout constraint
policy.WithTimeout(TimeSpan duration)

// Add retry callback for observability
policy.OnRetry(Action<Exception, int, TimeSpan> callback)

// Enable jitter (±25% random variation)
policy.WithJitter()

// Cap maximum delay (for exponential backoff)
policy.WithMaxDelay(TimeSpan maxDelay)

// Conditional retry based on result
policy.RetryIf<T>(Func<T, bool> predicate)

// Execute synchronously
policy.Execute(Action action, CancellationToken token = default)
policy.Execute<T>(Func<T> func, CancellationToken token = default)

// Execute asynchronously
policy.ExecuteAsync(Func<Task> func, CancellationToken token = default)
policy.ExecuteAsync<T>(Func<Task<T>> func, CancellationToken token = default)

// Execute with telemetry
policy.ExecuteWithResult(Action action, CancellationToken token = default)
policy.ExecuteWithResult<T>(Func<T> func, CancellationToken token = default)
policy.ExecuteWithResultAsync(Func<Task> func, CancellationToken token = default)
policy.ExecuteWithResultAsync<T>(Func<Task<T>> func, CancellationToken token = default)

// Fallback on failure
policy.Fallback<T>(Func<T> func, T fallbackValue, CancellationToken token = default)
policy.Fallback<T>(Func<T> func, Func<T> fallbackProvider, CancellationToken token = default)
policy.FallbackAsync<T>(Func<Task<T>> func, T fallbackValue, CancellationToken token = default)
policy.FallbackAsync<T>(Func<Task<T>> func, Func<Task<T>> fallbackProvider, CancellationToken token = default)
```

## Contributing

**Fox.RetryKit is intentionally lightweight and feature-focused.** The goal is to remain a simple, zero-dependency library for retry and timeout patterns.

### What We Welcome

- ✅ **Bug fixes** - Issues with existing functionality
- ✅ **Documentation improvements** - Clarifications, examples, typo fixes
- ✅ **Performance optimizations** - Without breaking API compatibility

### What We Generally Do Not Accept

- ❌ New dependencies or third-party packages
- ❌ Large feature additions that increase complexity
- ❌ Breaking API changes

If you want to propose a significant change, please open an issue first to discuss whether it aligns with the project's philosophy.

### Build Policy

The project enforces a **strict build policy** to ensure code quality:

- ❌ **No errors allowed** - Build must be error-free
- ❌ **No warnings allowed** - All compiler warnings must be resolved
- ❌ **No messages allowed** - Informational messages must be suppressed or addressed

All pull requests must pass this requirement.

### Code Quality Standards

Fox.RetryKit follows strict coding standards:

- **Comprehensive unit tests required** (xUnit + FluentAssertions)
- **Maximum test coverage required** - Aim for 100% line and branch coverage. Tests may only be omitted if they would introduce artificial complexity (e.g., testing unreachable code paths, framework internals, or compiler-generated code). Use `[ExcludeFromCodeCoverage]` sparingly and only for justified cases.
- **XML documentation for all public APIs** - Clear, concise documentation with examples
- **Follow Microsoft coding conventions** - See `.github/copilot-instructions.md` for project-specific style
- **Zero warnings, zero errors build policy** - Strict enforcement

### Code Style

- Follow the existing code style (see `.github/copilot-instructions.md`)
- Use file-scoped namespaces
- Enable nullable reference types
- Use expression-bodied members for simple properties/methods
- Private fields: camelCase without underscore prefix
- Add XML documentation decorators (98-character width)

### How to Contribute

1. Fork the repository
2. Create a feature branch from `main`
3. Follow the coding standards in `.github/copilot-instructions.md`
4. Write comprehensive unit tests (aim for 100% coverage)
5. Ensure all tests pass and build is clean (zero warnings/errors)
6. Submit a pull request

For detailed guidelines, see [CONTRIBUTING.md](CONTRIBUTING.md).

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Acknowledgments

Fox.RetryKit is part of the Fox.*Kit family of minimal utility libraries for .NET.
