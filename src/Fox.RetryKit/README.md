# Fox.RetryKit

A minimalist, dependency-free retry and timeout utility for C#. Build resilient applications with simple, type-safe retry policies.

## Key Features

- ✅ **Zero Dependencies** - No external dependencies, not even Microsoft.Extensions.*
- ✅ **Type-Safe** - Fluent API with compile-time safety
- ✅ **Flexible** - Fixed delays, exponential backoff, custom sequences
- ✅ **Timeout Support** - Built-in timeout with cancellation token integration
- ✅ **Exception Filtering** - Retry only specific exception types
- ✅ **Async-First** - Full support for async/await patterns
- ✅ **Observability** - OnRetry callbacks, telemetry support
- ✅ **Jitter** - Random variation to prevent thundering herd
- ✅ **Well-Documented** - Comprehensive XML documentation

## Installation

```bash
dotnet add package Fox.RetryKit
```

## Quick Start

### Basic Retry

```csharp
using Fox.RetryKit;

// Retry up to 3 times
var policy = RetryPolicy.Retry(3);

policy.Execute(() =>
{
    CallUnstableService();
});
```

### Retry with Delay

```csharp
// Retry 3 times with 500ms delay
var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(500));

await policy.ExecuteAsync(async () =>
{
    await CallUnstableApiAsync();
});
```

### Exponential Backoff

```csharp
// Retry 5 times with exponential backoff (100ms, 200ms, 400ms, 800ms, 1600ms)
var policy = RetryPolicy.ExponentialBackoff(5, TimeSpan.FromMilliseconds(100));

var result = await policy.ExecuteAsync(async () =>
{
    return await FetchDataAsync();
});
```

### Combined Policy

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

### OnRetry Callback

```csharp
var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(100))
    .OnRetry((exception, attempt, delay) =>
    {
        Console.WriteLine($"Retry {attempt} after {delay.TotalMilliseconds}ms: {exception.Message}");
    });
```

### Jitter (Prevent Thundering Herd)

```csharp
var policy = RetryPolicy.Retry(5, TimeSpan.FromMilliseconds(200))
    .WithJitter(); // Adds ±25% random variation
```

### MaxDelay Cap

```csharp
var policy = RetryPolicy.ExponentialBackoff(10, TimeSpan.FromMilliseconds(100))
    .WithMaxDelay(TimeSpan.FromSeconds(5)); // Cap at 5 seconds
```

### WaitAndRetry (Custom Delays)

```csharp
var delays = new[]
{
    TimeSpan.FromMilliseconds(100),
    TimeSpan.FromMilliseconds(250),
    TimeSpan.FromMilliseconds(500),
    TimeSpan.FromSeconds(1)
};

var policy = RetryPolicy.WaitAndRetry(delays);
```

### Fallback

```csharp
var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(100));

// Static fallback value
var result = policy.Fallback(() => GetRemoteData(), "default-value");

// Dynamic fallback provider
var result = policy.Fallback(() => GetPrimaryData(), () => GetCachedData());
```

### RetryIf (Conditional Retry)

```csharp
var policy = RetryPolicy.Retry(5, TimeSpan.FromMilliseconds(100))
    .RetryIf<HttpResponseMessage>(response => !response.IsSuccessStatusCode);
```

### RetryResult (Telemetry)

```csharp
var result = await policy.ExecuteWithResultAsync(async () => await FetchDataAsync());

Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Attempts: {result.Attempts}");
Console.WriteLine($"Duration: {result.TotalDuration}");
```

## Real-World Examples

### HTTP API Calls

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
});
```

### File I/O

```csharp
var filePolicy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(100))
    .Handle<IOException>();

var content = filePolicy.Execute(() => File.ReadAllText("config.json"));
```

## API Reference

### Static Factories
- `Retry(int count)` - Immediate retry
- `Retry(int count, TimeSpan delay)` - Fixed delay retry
- `ExponentialBackoff(int retries, TimeSpan initialDelay)` - Exponential backoff
- `Timeout(TimeSpan duration)` - Timeout constraint
- `WaitAndRetry(IEnumerable<TimeSpan> delays)` - Custom delay sequence

### Fluent Methods
- `Handle<TException>()` - Exception filtering
- `WithTimeout(TimeSpan duration)` - Add timeout
- `OnRetry(Action<Exception, int, TimeSpan>)` - Retry callback
- `WithJitter()` - Add ±25% random variation
- `WithMaxDelay(TimeSpan maxDelay)` - Cap maximum delay
- `RetryIf<T>(Func<T, bool> predicate)` - Conditional retry

### Execution Methods
- `Execute(Action action)` - Synchronous execution
- `Execute<T>(Func<T> func)` - Synchronous with result
- `ExecuteAsync(Func<Task> func)` - Asynchronous execution
- `ExecuteAsync<T>(Func<Task<T>> func)` - Asynchronous with result
- `ExecuteWithResult()` / `ExecuteWithResultAsync()` - With telemetry
- `Fallback<T>()` / `FallbackAsync<T>()` - Graceful degradation

## License

MIT License - Copyright (c) 2026 Károly Akácz

## Links

- **GitHub**: https://github.com/akikari/Fox.RetryKit
- **NuGet**: https://www.nuget.org/packages/Fox.RetryKit/
- **Issues**: https://github.com/akikari/Fox.RetryKit/issues
## Contributing

Contributions are welcome! Please open an issue or pull request in the GitHub repository.

## Support

If you like Fox.OptionKit, please give it a ⭐ on the GitHub repository!
