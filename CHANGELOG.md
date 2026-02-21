# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

_No unreleased changes yet._

## [1.0.0] - 2026-02-21

Initial release of Fox.RetryKit - a minimalist, dependency-free retry and timeout utility for C#.

### Added

#### Core Features
- `RetryPolicy` class for defining retry behavior with fluent API
- `Retry(int count)` and `Retry(int count, TimeSpan delay)` static factories
- `ExponentialBackoff(int retries, TimeSpan initialDelay)` for exponential backoff strategy
- `Timeout(TimeSpan duration)` static factory for timeout constraints
- `Handle<TException>()` method for exception type filtering
- `WithTimeout(TimeSpan duration)` extension method
- Full async/await support with `ExecuteAsync()` methods
- Full CancellationToken support for all execution methods
- Zero external dependencies

#### Extended Features
- `OnRetry(Action<Exception, int, TimeSpan> callback)` for observability and logging integration
- `WithJitter()` method to add ±25% random variation to delays
- `WithMaxDelay(TimeSpan maxDelay)` to limit exponential backoff growth
- `WaitAndRetry(IEnumerable<TimeSpan> delays)` for custom delay sequences
- `Fallback<T>()` and `FallbackAsync<T>()` for graceful degradation
- `RetryIf<T>(Func<T, bool> predicate)` for conditional retry based on result values
- `ExecuteWithResult()` and `ExecuteWithResultAsync()` returning detailed retry metrics

#### Public Types
- `RetryPolicyBase` - Abstract base class for all retry policies (new in architecture refactor)
- `RetryPolicy` - Main policy class, now inherits from `RetryPolicyBase`
- `CustomDelayRetryPolicy` - Specialized policy for custom delay sequences, inherits from `RetryPolicyBase`
- `RetryIfPolicy<T>` - Specialized policy for conditional retry, inherits from `RetryPolicyBase`
- `RetryResult<T>` - Telemetry result with value, attempts, duration, and last exception
- `RetryResult` - Non-generic telemetry result for void operations
- `DelayStrategy` enum (Fixed, Exponential)

#### Documentation & Samples
- Comprehensive README.md with quick start and API reference
- Demo application with 14 working examples
- Real-world scenarios (HTTP, database, file I/O, message queues)
- MIT License

#### Tests
- 285 comprehensive unit tests (100% passing)
- Test coverage: Retry, Delay, Timeout, Exception Filtering, Cancellation, Async, OnRetry, Jitter, MaxDelay, WaitAndRetry, Fallback, RetryIf, RetryResult
- xUnit + FluentAssertions
- Multi-targeting: .NET 8.0, .NET 9.0, .NET 10.0

#### Build & Package
- Multi-targeting: net8.0, net9.0, net10.0
- NuGet package metadata with icon and README
- Symbol packages (.snupkg) for debugging
- XML documentation file included

### Technical Details
- Zero dependencies (production code)
- Nullable reference types enabled
- File-scoped namespaces
- Complete XML documentation in English for all members (public, internal, private)
- NuGet package includes XML documentation for IntelliSense support
- Production-ready code quality
- Clean architecture with `RetryPolicyBase` abstract base class
- SOLID principles compliance (OCP, LSP)
- Covariant return types for type-safe fluent API
- Design-time safety with compile-time error prevention

