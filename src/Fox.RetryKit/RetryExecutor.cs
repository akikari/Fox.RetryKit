//==================================================================================================
// Internal executor that implements retry logic with delay, timeout, and exception handling.
// Handles both synchronous and asynchronous execution with cancellation token support.
//==================================================================================================

namespace Fox.RetryKit;

//==================================================================================================
/// <summary>
/// Internal executor for retry operations with support for delays, timeouts, and exception filtering.
/// </summary>
//==================================================================================================
internal static class RetryExecutor
{
    #region Public Methods

    //==============================================================================================
    /// <summary>
    /// Executes an action with retry logic according to the specified policy.
    /// </summary>
    /// <param name="policy">The retry policy to apply.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    //==============================================================================================
    public static void Execute(RetryPolicyBase policy, Action action, CancellationToken cancellationToken)
    {
        Execute(policy, () => { action(); return true; }, cancellationToken);
    }

    //==============================================================================================
    /// <summary>
    /// Executes a function with retry logic according to the specified policy.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="policy">The retry policy to apply.</param>
    /// <param name="func">The function to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the function execution.</returns>
    //==============================================================================================
    public static T Execute<T>(RetryPolicyBase policy, Func<T> func, CancellationToken cancellationToken)
    {
        using var timeoutCts = CreateTimeoutCancellationTokenSource(policy.TimeoutDuration);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts?.Token ?? default);
        var token = linkedCts.Token;

        Exception? lastException = null;
        int attempt = 0;

        while (attempt <= policy.RetryCount)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                return func();
            }
            catch (Exception ex) when (ShouldRetry(policy, ex, attempt, policy.RetryCount))
            {
                lastException = ex;
                attempt++;

                if (attempt <= policy.RetryCount)
                {
                    var delay = CalculateDelay(policy, attempt);
                    policy.OnRetryCallback?.Invoke(ex, attempt, delay);

                    if (delay > TimeSpan.Zero)
                    {
                        Thread.Sleep(delay);
                    }
                }
            }
        }

        throw lastException ?? new InvalidOperationException("Retry operation failed.");
    }

    //==============================================================================================
    /// <summary>
    /// Executes an asynchronous function with retry logic according to the specified policy.
    /// </summary>
    /// <param name="policy">The retry policy to apply.</param>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    //==============================================================================================
    public static async Task ExecuteAsync(RetryPolicyBase policy, Func<Task> func, CancellationToken cancellationToken)
    {
        await ExecuteAsync(policy, async () => { await func(); return true; }, cancellationToken);
    }

    //==============================================================================================
    /// <summary>
    /// Executes an asynchronous function with retry logic according to the specified policy.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="policy">The retry policy to apply.</param>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with result.</returns>
    //==============================================================================================
    public static async Task<T> ExecuteAsync<T>(RetryPolicyBase policy, Func<Task<T>> func, CancellationToken cancellationToken)
    {
        using var timeoutCts = CreateTimeoutCancellationTokenSource(policy.TimeoutDuration);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts?.Token ?? default);
        var token = linkedCts.Token;

        Exception? lastException = null;
        int attempt = 0;

        while (attempt <= policy.RetryCount)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var task = func();

                if (policy.TimeoutDuration.HasValue)
                {
                    var timeoutTask = Task.Delay(policy.TimeoutDuration.Value, token);
                    var completedTask = await Task.WhenAny(task, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        throw new OperationCanceledException("Operation timed out.");
                    }
                }

                return await task;
            }
            catch (Exception ex) when (ShouldRetry(policy, ex, attempt, policy.RetryCount))
            {
                lastException = ex;
                attempt++;

                if (attempt <= policy.RetryCount)
                {
                    var delay = CalculateDelay(policy, attempt);
                    policy.OnRetryCallback?.Invoke(ex, attempt, delay);

                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, token);
                    }
                }
            }
        }

        throw lastException ?? new InvalidOperationException("Retry operation failed.");
    }

    //==============================================================================================
    /// <summary>
    /// Executes an action with retry logic and returns detailed telemetry.
    /// </summary>
    /// <param name="policy">The retry policy to apply.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result object containing success status, attempt count, and duration.</returns>
    //==============================================================================================
    public static RetryResult ExecuteWithResult(RetryPolicyBase policy, Action action, CancellationToken cancellationToken)
    {
        bool wrappedFunc() { action(); return true; }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = ExecuteWithResult(policy, wrappedFunc, cancellationToken);
        return new RetryResult(result.Success, result.Attempts, result.TotalDuration, result.LastException);
    }

    //==============================================================================================
    /// <summary>
    /// Executes a function with retry logic and returns detailed telemetry.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="policy">The retry policy to apply.</param>
    /// <param name="func">The function to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result object containing value, success status, attempt count, and duration.</returns>
    //==============================================================================================
    public static RetryResult<T> ExecuteWithResult<T>(RetryPolicyBase policy, Func<T> func, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        using var timeoutCts = CreateTimeoutCancellationTokenSource(policy.TimeoutDuration);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts?.Token ?? default);
        var token = linkedCts.Token;

        Exception? lastException = null;
        int attempt = 0;

        while (attempt <= policy.RetryCount)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var value = func();
                stopwatch.Stop();
                return new RetryResult<T>(true, value, attempt + 1, stopwatch.Elapsed, null);
            }
            catch (Exception ex) when (ShouldRetry(policy, ex, attempt, policy.RetryCount))
            {
                lastException = ex;
                attempt++;

                if (attempt <= policy.RetryCount)
                {
                    var delay = CalculateDelay(policy, attempt);
                    policy.OnRetryCallback?.Invoke(ex, attempt, delay);

                    if (delay > TimeSpan.Zero)
                    {
                        Thread.Sleep(delay);
                    }
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new RetryResult<T>(false, default, attempt + 1, stopwatch.Elapsed, ex);
            }
        }

        stopwatch.Stop();
        return new RetryResult<T>(false, default, attempt + 1, stopwatch.Elapsed, lastException);
    }

    //==============================================================================================
    /// <summary>
    /// Executes an asynchronous function with retry logic and returns detailed telemetry.
    /// </summary>
    /// <param name="policy">The retry policy to apply.</param>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task with result object containing success status, attempt count, and duration.</returns>
    //==============================================================================================
    public static async Task<RetryResult> ExecuteWithResultAsync(RetryPolicyBase policy, Func<Task> func, CancellationToken cancellationToken)
    {
        async Task<bool> wrappedFunc() { await func(); return true; }

        var result = await ExecuteWithResultAsync(policy, wrappedFunc, cancellationToken);
        return new RetryResult(result.Success, result.Attempts, result.TotalDuration, result.LastException);
    }

    //==============================================================================================
    /// <summary>
    /// Executes an asynchronous function with retry logic and returns detailed telemetry.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="policy">The retry policy to apply.</param>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task with result object containing value, success status, attempt count, and duration.</returns>
    //==============================================================================================
    public static async Task<RetryResult<T>> ExecuteWithResultAsync<T>(RetryPolicyBase policy, Func<Task<T>> func, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        using var timeoutCts = CreateTimeoutCancellationTokenSource(policy.TimeoutDuration);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts?.Token ?? default);
        var token = linkedCts.Token;

        Exception? lastException = null;
        int attempt = 0;

        while (attempt <= policy.RetryCount)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var task = func();

                if (policy.TimeoutDuration.HasValue)
                {
                    var timeoutTask = Task.Delay(policy.TimeoutDuration.Value, token);
                    var completedTask = await Task.WhenAny(task, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        throw new OperationCanceledException("Operation timed out.");
                    }
                }

                var value = await task;
                stopwatch.Stop();
                return new RetryResult<T>(true, value, attempt + 1, stopwatch.Elapsed, null);
            }
            catch (Exception ex) when (ShouldRetry(policy, ex, attempt, policy.RetryCount))
            {
                lastException = ex;
                attempt++;

                if (attempt <= policy.RetryCount)
                {
                    var delay = CalculateDelay(policy, attempt);
                    policy.OnRetryCallback?.Invoke(ex, attempt, delay);

                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, token);
                    }
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new RetryResult<T>(false, default, attempt + 1, stopwatch.Elapsed, ex);
            }
        }

        stopwatch.Stop();
        return new RetryResult<T>(false, default, attempt + 1, stopwatch.Elapsed, lastException);
    }

    //==============================================================================================
    /// <summary>
    /// Executes a function with conditional retry logic based on result predicate.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="policy">The retry-if policy to apply.</param>
    /// <param name="func">The function to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the function execution.</returns>
    //==============================================================================================
    public static T Execute<T>(RetryIfPolicy<T> policy, Func<T> func, CancellationToken cancellationToken)
    {
        using var timeoutCts = CreateTimeoutCancellationTokenSource(policy.TimeoutDuration);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts?.Token ?? default);
        var token = linkedCts.Token;

        Exception? lastException = null;
        int attempt = 0;

        while (attempt <= policy.RetryCount)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var result = func();

                if (attempt < policy.RetryCount && policy.RetryPredicate(result))
                {
                    attempt++;
                    var delay = CalculateDelay(policy, attempt);
                    policy.OnRetryCallback?.Invoke(new InvalidOperationException("Retry condition met"), attempt, delay);

                    if (delay > TimeSpan.Zero)
                    {
                        Thread.Sleep(delay);
                    }

                    continue;
                }

                return result;
            }
            catch (Exception ex) when (ShouldRetry(policy, ex, attempt, policy.RetryCount))
            {
                lastException = ex;
                attempt++;

                if (attempt <= policy.RetryCount)
                {
                    var delay = CalculateDelay(policy, attempt);
                    policy.OnRetryCallback?.Invoke(ex, attempt, delay);

                    if (delay > TimeSpan.Zero)
                    {
                        Thread.Sleep(delay);
                    }
                }
            }
        }

        throw lastException ?? new InvalidOperationException("Retry operation failed.");
    }

    //==============================================================================================
    /// <summary>
    /// Executes an async function with conditional retry logic based on result predicate.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="policy">The retry-if policy to apply.</param>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with result.</returns>
    //==============================================================================================
    public static async Task<T> ExecuteAsync<T>(RetryIfPolicy<T> policy, Func<Task<T>> func, CancellationToken cancellationToken)
    {
        using var timeoutCts = CreateTimeoutCancellationTokenSource(policy.TimeoutDuration);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts?.Token ?? default);
        var token = linkedCts.Token;

        Exception? lastException = null;
        int attempt = 0;

        while (attempt <= policy.RetryCount)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var task = func();

                if (policy.TimeoutDuration.HasValue)
                {
                    var timeoutTask = Task.Delay(policy.TimeoutDuration.Value, token);
                    var completedTask = await Task.WhenAny(task, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        throw new OperationCanceledException("Operation timed out.");
                    }
                }

                var result = await task;

                if (attempt < policy.RetryCount && policy.RetryPredicate(result))
                {
                    attempt++;
                    var delay = CalculateDelay(policy, attempt);
                    policy.OnRetryCallback?.Invoke(new InvalidOperationException("Retry condition met"), attempt, delay);

                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, token);
                    }

                    continue;
                }

                return result;
            }
            catch (Exception ex) when (ShouldRetry(policy, ex, attempt, policy.RetryCount))
            {
                lastException = ex;
                attempt++;

                if (attempt <= policy.RetryCount)
                {
                    var delay = CalculateDelay(policy, attempt);
                    policy.OnRetryCallback?.Invoke(ex, attempt, delay);

                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, token);
                    }
                }
            }
        }

        throw lastException ?? new InvalidOperationException("Retry operation failed.");
    }

    #endregion

    #region Private Methods

    //==============================================================================================
    /// <summary>
    /// Determines whether the operation should be retried based on the exception and attempt count.
    /// </summary>
    /// <param name="policy">The retry policy to evaluate.</param>
    /// <param name="exception">The exception that was thrown.</param>
    /// <param name="currentAttempt">The current attempt number.</param>
    /// <param name="maxRetries">The maximum number of retries allowed.</param>
    /// <returns>True if the operation should be retried; otherwise, false.</returns>
    //==============================================================================================
    private static bool ShouldRetry(RetryPolicyBase policy, Exception exception, int currentAttempt, int maxRetries)
    {
        if (currentAttempt >= maxRetries)
        {
            return false;
        }

        if (policy.HandledExceptions.Count == 0)
        {
            return true;
        }

        var exceptionType = exception.GetType();

        foreach (var handledType in policy.HandledExceptions)
        {
            if (handledType.IsAssignableFrom(exceptionType))
            {
                return true;
            }
        }

        return false;
    }

    //==============================================================================================
    /// <summary>
    /// Calculates the delay duration for a specific retry attempt.
    /// </summary>
    /// <param name="policy">The retry policy containing delay configuration.</param>
    /// <param name="attempt">The attempt number (1-based).</param>
    /// <returns>The calculated delay duration, potentially adjusted by jitter and max delay.</returns>
    //==============================================================================================
    private static TimeSpan CalculateDelay(RetryPolicyBase policy, int attempt)
    {
        TimeSpan baseDelay;

        if (policy is CustomDelayRetryPolicy customPolicy)
        {
            baseDelay = customPolicy.GetDelayForAttempt(attempt);
        }
        else
        {
            baseDelay = policy.Strategy switch
            {
                DelayStrategy.Fixed => policy.Delay,
                DelayStrategy.Exponential => TimeSpan.FromMilliseconds(policy.Delay.TotalMilliseconds * Math.Pow(2, attempt - 1)),
                _ => TimeSpan.Zero
            };
        }

        if (policy.MaxDelay.HasValue && baseDelay > policy.MaxDelay.Value)
        {
            baseDelay = policy.MaxDelay.Value;
        }

        if (policy.UseJitter && baseDelay > TimeSpan.Zero)
        {
            var jitterFactor = 0.75 + (Random.Shared.NextDouble() * 0.5);
            baseDelay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * jitterFactor);
        }

        return baseDelay;
    }

    //==============================================================================================
    /// <summary>
    /// Creates a cancellation token source with the specified timeout, or null if no timeout.
    /// </summary>
    /// <param name="timeout">The optional timeout duration.</param>
    /// <returns>A cancellation token source with timeout, or null if timeout is not specified.</returns>
    //==============================================================================================
    private static CancellationTokenSource? CreateTimeoutCancellationTokenSource(TimeSpan? timeout)
    {
        if (timeout.HasValue)
        {
            return new CancellationTokenSource(timeout.Value);
        }

        return null;
    }

    #endregion
}
