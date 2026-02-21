//==================================================================================================
// Demo application showing Fox.RetryKit features and usage examples.
// Demonstrates retry, timeout, exponential backoff, and exception filtering.
//==================================================================================================
using Fox.RetryKit;

Console.WriteLine("=== Fox.RetryKit Demo ===\n");

//==================================================================================================
// Example 1: Basic Retry
//==================================================================================================
Console.WriteLine("Example 1: Basic Retry (3 attempts)");
var retryPolicy = RetryPolicy.Retry(3);
var attempt1 = 0;

try
{
    retryPolicy.Execute(() =>
    {
        attempt1++;
        Console.WriteLine($"  Attempt {attempt1}");

        if (attempt1 < 3)
        {
            throw new InvalidOperationException("Temporary failure");
        }

        Console.WriteLine("  Success!");
    });
}
catch (Exception ex)
{
    Console.WriteLine($"  Failed: {ex.Message}");
}

Console.WriteLine();

//==================================================================================================
// Example 2: Retry with Fixed Delay
//==================================================================================================
Console.WriteLine("Example 2: Retry with Fixed Delay (3 retries, 200ms delay)");
var delayPolicy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(200));
var attempt2 = 0;

try
{
    delayPolicy.Execute(() =>
    {
        attempt2++;
        Console.WriteLine($"  Attempt {attempt2} at {DateTime.Now:HH:mm:ss.fff}");

        if (attempt2 < 3)
        {
            throw new InvalidOperationException("Temporary failure");
        }

        Console.WriteLine("  Success!");
    });
}
catch (Exception ex)
{
    Console.WriteLine($"  Failed: {ex.Message}");
}

Console.WriteLine();

//==================================================================================================
// Example 3: Exponential Backoff
//==================================================================================================
Console.WriteLine("Example 3: Exponential Backoff (3 retries, 100ms initial delay)");
var backoffPolicy = RetryPolicy.ExponentialBackoff(3, TimeSpan.FromMilliseconds(100));
var attempt3 = 0;

try
{
    backoffPolicy.Execute(() =>
    {
        attempt3++;
        Console.WriteLine($"  Attempt {attempt3} at {DateTime.Now:HH:mm:ss.fff}");

        if (attempt3 < 4)
        {
            throw new InvalidOperationException("Temporary failure");
        }

        Console.WriteLine("  Success!");
    });
}
catch (Exception ex)
{
    Console.WriteLine($"  Failed: {ex.Message}");
}

Console.WriteLine();

//==================================================================================================
// Example 4: Timeout
//==================================================================================================
Console.WriteLine("Example 4: Timeout (500ms limit)");
var timeoutPolicy = RetryPolicy.Timeout(TimeSpan.FromMilliseconds(500));

try
{
    await timeoutPolicy.ExecuteAsync(async () =>
    {
        Console.WriteLine("  Starting long operation...");
        await Task.Delay(TimeSpan.FromSeconds(5));
        Console.WriteLine("  Operation completed");
    });
}
catch (OperationCanceledException)
{
    Console.WriteLine("  Operation timed out!");
}

Console.WriteLine();

//==================================================================================================
// Example 5: Exception Filtering
//==================================================================================================
Console.WriteLine("Example 5: Exception Filtering (only retry InvalidOperationException)");
var filterPolicy = RetryPolicy.Retry(3).Handle<InvalidOperationException>();
var attempt5 = 0;

try
{
    filterPolicy.Execute(() =>
    {
        attempt5++;
        Console.WriteLine($"  Attempt {attempt5}");

        if (attempt5 == 1)
        {
            throw new InvalidOperationException("Handled exception - will retry");
        }

        if (attempt5 == 2)
        {
            throw new ArgumentException("Unhandled exception - will not retry");
        }
    });
}
catch (ArgumentException)
{
    Console.WriteLine("  Failed with unhandled exception (no retry)");
}

Console.WriteLine();

//==================================================================================================
// Example 6: Combined: Retry + Delay + Timeout + Exception Filtering
//==================================================================================================
Console.WriteLine("Example 6: Combined (3 retries, 100ms delay, 1s timeout, handle IOException)");
var combinedPolicy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(100))
    .Handle<IOException>()
    .WithTimeout(TimeSpan.FromSeconds(1));

var attempt6 = 0;

try
{
    var result = await combinedPolicy.ExecuteAsync(async () =>
    {
        attempt6++;
        Console.WriteLine($"  Attempt {attempt6}");
        await Task.Delay(10);

        if (attempt6 < 3)
        {
            throw new IOException("Temporary IO failure");
        }

        return "Success!";
    });

    Console.WriteLine($"  Result: {result}");
}
catch (Exception ex)
{
    Console.WriteLine($"  Failed: {ex.GetType().Name}");
}

Console.WriteLine();

//==================================================================================================
// Example 7: Real-world scenario - HTTP Request Simulation
//==================================================================================================
Console.WriteLine("Example 7: Real-world - Simulated HTTP Request with Retry");
var httpPolicy = RetryPolicy.Retry(5, TimeSpan.FromMilliseconds(500))
    .Handle<HttpRequestException>()
    .WithTimeout(TimeSpan.FromSeconds(10));

var httpAttempt = 0;

try
{
    var response = await httpPolicy.ExecuteAsync(async () =>
    {
        httpAttempt++;
        Console.WriteLine($"  HTTP Request attempt {httpAttempt}");
        await Task.Delay(50);

        if (httpAttempt < 3)
        {
            throw new HttpRequestException("503 Service Unavailable");
        }

        return "200 OK";
    });

    Console.WriteLine($"  Response: {response}");
}
catch (Exception ex)
{
    Console.WriteLine($"  Failed: {ex.Message}");
}

Console.WriteLine();

//==================================================================================================
// Example 8: OnRetry Callback with Logging
//==================================================================================================
Console.WriteLine("Example 8: OnRetry Callback (logging retry attempts)");
var onRetryPolicy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(100))
    .OnRetry((exception, attempt, delay) =>
    {
        Console.WriteLine($"  [OnRetry] Attempt {attempt} failed: {exception.Message}");
        Console.WriteLine($"  [OnRetry] Waiting {delay.TotalMilliseconds}ms before next retry...");
    });

var attempt8 = 0;

try
{
    onRetryPolicy.Execute(() =>
    {
        attempt8++;
        Console.WriteLine($"  Executing attempt {attempt8}");

        if (attempt8 < 3)
        {
            throw new InvalidOperationException($"Failure #{attempt8}");
        }

        Console.WriteLine("  Success!");
    });
}
catch (Exception ex)
{
    Console.WriteLine($"  Final failure: {ex.Message}");
}

Console.WriteLine();

//==================================================================================================
// Example 9: Jitter (Random Variation)
//==================================================================================================
Console.WriteLine("Example 9: Jitter (±25% random variation to prevent thundering herd)");
var jitterPolicy = RetryPolicy.Retry(5, TimeSpan.FromMilliseconds(200))
    .WithJitter()
    .OnRetry((_, attempt, delay) =>
    {
        Console.WriteLine($"  Retry {attempt} with jittered delay: {delay.TotalMilliseconds:F1}ms");
    });

var attempt9 = 0;

try
{
    jitterPolicy.Execute(() =>
    {
        attempt9++;

        if (attempt9 < 4)
        {
            throw new InvalidOperationException("Transient error");
        }
    });

    Console.WriteLine("  Success after retries with jitter!");
}
catch (Exception ex)
{
    Console.WriteLine($"  Failed: {ex.Message}");
}

Console.WriteLine();

//==================================================================================================
// Example 10: MaxDelay Cap
//==================================================================================================
Console.WriteLine("Example 10: MaxDelay Cap (limit exponential backoff growth)");
var maxDelayPolicy = RetryPolicy.ExponentialBackoff(6, TimeSpan.FromMilliseconds(100))
    .WithMaxDelay(TimeSpan.FromMilliseconds(500))
    .OnRetry((_, attempt, delay) =>
    {
        Console.WriteLine($"  Retry {attempt}: delay = {delay.TotalMilliseconds}ms");
    });

var attempt10 = 0;

try
{
    maxDelayPolicy.Execute(() =>
    {
        attempt10++;

        if (attempt10 < 5)
        {
            throw new InvalidOperationException("Transient error");
        }
    });

    Console.WriteLine("  Success with capped delays!");
}
catch (Exception ex)
{
    Console.WriteLine($"  Failed: {ex.Message}");
}

Console.WriteLine();

//==================================================================================================
// Example 11: WaitAndRetry (Custom Delay Sequence)
//==================================================================================================
Console.WriteLine("Example 11: WaitAndRetry (custom delay progression)");
var customDelays = new[]
{
    TimeSpan.FromMilliseconds(100),
    TimeSpan.FromMilliseconds(250),
    TimeSpan.FromMilliseconds(500),
    TimeSpan.FromSeconds(1)
};

var waitAndRetryPolicy = RetryPolicy.WaitAndRetry(customDelays)
    .OnRetry((_, attempt, delay) =>
    {
        Console.WriteLine($"  Retry {attempt} with custom delay: {delay.TotalMilliseconds}ms");
    });

var attempt11 = 0;

try
{
    waitAndRetryPolicy.Execute(() =>
    {
        attempt11++;

        if (attempt11 < 4)
        {
            throw new InvalidOperationException("Transient error");
        }
    });

    Console.WriteLine("  Success with custom delay sequence!");
}
catch (Exception ex)
{
    Console.WriteLine($"  Failed: {ex.Message}");
}

Console.WriteLine();

//==================================================================================================
// Example 12: Fallback (Graceful Degradation)
//==================================================================================================
Console.WriteLine("Example 12: Fallback (graceful degradation with default value)");
var fallbackPolicy = RetryPolicy.Retry(2, TimeSpan.FromMilliseconds(100));

var result12 = fallbackPolicy.Fallback(() =>
{
    Console.WriteLine("  Attempting primary operation...");
    throw new InvalidOperationException("Primary service unavailable");
}, "Fallback Value");

Console.WriteLine($"  Result: {result12}");
Console.WriteLine();

Console.WriteLine("Example 12b: Fallback with Provider Function");
var result12b = fallbackPolicy.Fallback(() =>
{
    Console.WriteLine("  Attempting primary data source...");
    throw new InvalidOperationException("Primary unavailable");
}, () =>
{
    Console.WriteLine("  Using fallback data source...");
    return "Cached Data";
});

Console.WriteLine($"  Result: {result12b}");
Console.WriteLine();

//==================================================================================================
// Example 13: RetryIf (Conditional Retry)
//==================================================================================================
Console.WriteLine("Example 13: RetryIf (conditional retry based on result)");
var retryIfPolicy = RetryPolicy.Retry(5, TimeSpan.FromMilliseconds(100))
    .RetryIf<int>(result => result < 100)
    .OnRetry((_, attempt, _) =>
    {
        Console.WriteLine($"  Retry {attempt}: result was below threshold");
    });

var attempt13 = 0;

var result13 = retryIfPolicy.Execute(() =>
{
    attempt13++;
    var value = attempt13 * 25;
    Console.WriteLine($"  Attempt {attempt13}: returned value = {value}");
    return value;
});

Console.WriteLine($"  Final result: {result13} (threshold reached after {attempt13} attempts)");
Console.WriteLine();

//==================================================================================================
// Example 14: RetryResult (Telemetry)
//==================================================================================================
Console.WriteLine("Example 14: RetryResult (detailed telemetry and metrics)");
var telemetryPolicy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(150));

var attempt14 = 0;

var result14 = telemetryPolicy.ExecuteWithResult(() =>
{
    attempt14++;
    Console.WriteLine($"  Executing attempt {attempt14}");

    if (attempt14 < 3)
    {
        throw new InvalidOperationException($"Temporary failure #{attempt14}");
    }

    return "Operation completed successfully";
});

Console.WriteLine($"\n  Telemetry:");
Console.WriteLine($"    Success: {result14.Success}");
Console.WriteLine($"    Value: {result14.Value}");
Console.WriteLine($"    Total Attempts: {result14.Attempts}");
Console.WriteLine($"    Total Duration: {result14.TotalDuration.TotalMilliseconds:F0}ms");
Console.WriteLine($"    Last Exception: {result14.LastException?.Message ?? "None"}");
Console.WriteLine();

Console.WriteLine("Example 14b: RetryResult with Failure");
var attempt14b = 0;

var result14b = telemetryPolicy.ExecuteWithResult<string>(() =>
{
    attempt14b++;

    throw new InvalidOperationException("Persistent failure");
});

Console.WriteLine($"\n  Telemetry (Failed Operation):");
Console.WriteLine($"    Success: {result14b.Success}");
Console.WriteLine($"    Value: {result14b.Value ?? "(null)"}");
Console.WriteLine($"    Total Attempts: {result14b.Attempts}");
Console.WriteLine($"    Total Duration: {result14b.TotalDuration.TotalMilliseconds:F0}ms");
Console.WriteLine($"    Last Exception: {result14b.LastException?.Message ?? "None"}");

Console.WriteLine("\n=== Demo Complete ===");
