//==================================================================================================
// Base class for all retry policies with shared configuration and execution logic.
// Provides abstract factory methods for derived policy types with covariant return types.
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
    #region Properties

    //==============================================================================================
    /// <summary>
    /// Gets the maximum number of retry attempts.
    /// </summary>
    //==============================================================================================
    public int RetryCount { get; }

    //==============================================================================================
    /// <summary>
    /// Gets the delay strategy between retry attempts.
    /// </summary>
    //==============================================================================================
    public DelayStrategy Strategy { get; }

    //==============================================================================================
    /// <summary>
    /// Gets the base delay duration between retry attempts.
    /// </summary>
    //==============================================================================================
    public TimeSpan Delay { get; }

    //==============================================================================================
    /// <summary>
    /// Gets the timeout duration for the entire operation (including all retries).
    /// </summary>
    //==============================================================================================
    public TimeSpan? TimeoutDuration { get; }

    //==============================================================================================
    /// <summary>
    /// Gets the collection of exception types that should trigger a retry.
    /// </summary>
    //==============================================================================================
    public IReadOnlyCollection<Type> HandledExceptions { get; }

    //==============================================================================================
    /// <summary>
    /// Gets the callback action invoked before each retry attempt.
    /// </summary>
    //==============================================================================================
    public Action<Exception, int, TimeSpan>? OnRetryCallback { get; }

    //==============================================================================================
    /// <summary>
    /// Gets a value indicating whether jitter should be applied to delays.
    /// </summary>
    //==============================================================================================
    public bool UseJitter { get; }

    //==============================================================================================
    /// <summary>
    /// Gets the maximum delay cap for exponential backoff.
    /// </summary>
    //==============================================================================================
    public TimeSpan? MaxDelay { get; }

    #endregion

    #region Constructors

    //==============================================================================================
    /// <summary>
    /// Initializes a new instance of the retry policy base with specified parameters.
    /// </summary>
    /// <param name="retryCount">The maximum number of retry attempts.</param>
    /// <param name="strategy">The delay strategy to use.</param>
    /// <param name="delay">The base delay duration.</param>
    /// <param name="timeout">The optional timeout duration.</param>
    /// <param name="handledExceptions">The collection of exception types to handle.</param>
    /// <param name="onRetryCallback">The optional callback invoked before each retry.</param>
    /// <param name="useJitter">Whether to apply jitter to delays.</param>
    /// <param name="maxDelay">The optional maximum delay cap.</param>
    //==============================================================================================
    internal RetryPolicyBase(int retryCount, DelayStrategy strategy, TimeSpan delay, TimeSpan? timeout, IReadOnlyCollection<Type> handledExceptions, Action<Exception, int, TimeSpan>? onRetryCallback, bool useJitter, TimeSpan? maxDelay)
    {
        RetryCount = retryCount;
        Strategy = strategy;
        Delay = delay;
        TimeoutDuration = timeout;
        HandledExceptions = handledExceptions;
        OnRetryCallback = onRetryCallback;
        UseJitter = useJitter;
        MaxDelay = maxDelay;
    }

    #endregion

    #region Public Methods

    //==============================================================================================
    /// <summary>
    /// Configures a callback to be invoked before each retry attempt.
    /// </summary>
    /// <param name="callback">The callback action that receives exception, attempt number, and delay.</param>
    /// <returns>A new retry policy with the callback configured.</returns>
    //==============================================================================================
    public abstract RetryPolicyBase OnRetry(Action<Exception, int, TimeSpan> callback);

    //==============================================================================================
    /// <summary>
    /// Enables jitter (random variation) for retry delays to prevent thundering herd.
    /// </summary>
    /// <returns>A new retry policy with jitter enabled.</returns>
    //==============================================================================================
    public abstract RetryPolicyBase WithJitter();

    //==============================================================================================
    /// <summary>
    /// Sets a maximum delay cap for exponential backoff to prevent excessively long waits.
    /// </summary>
    /// <param name="maxDelay">The maximum delay duration.</param>
    /// <returns>A new retry policy with maximum delay configured.</returns>
    //==============================================================================================
    public abstract RetryPolicyBase WithMaxDelay(TimeSpan maxDelay);

    //==============================================================================================
    /// <summary>
    /// Executes the specified action with retry logic according to this policy.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when operation is cancelled or times out.</exception>
    //==============================================================================================
    public void Execute(Action action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        RetryExecutor.Execute(this, action, cancellationToken);
    }

    //==============================================================================================
    /// <summary>
    /// Executes the specified function with retry logic according to this policy.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The result of the function execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when func is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when operation is cancelled or times out.</exception>
    //==============================================================================================
    public T Execute<T>(Func<T> func, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);

        return RetryExecutor.Execute(this, func, cancellationToken);
    }

    //==============================================================================================
    /// <summary>
    /// Executes the specified asynchronous function with retry logic according to this policy.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when func is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when operation is cancelled or times out.</exception>
    //==============================================================================================
    public Task ExecuteAsync(Func<Task> func, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);

        return RetryExecutor.ExecuteAsync(this, func, cancellationToken);
    }

    //==============================================================================================
    /// <summary>
    /// Executes the specified asynchronous function with retry logic according to this policy.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when func is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when operation is cancelled or times out.</exception>
    //==============================================================================================
    public Task<T> ExecuteAsync<T>(Func<Task<T>> func, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);

        return RetryExecutor.ExecuteAsync(this, func, cancellationToken);
    }

    //==============================================================================================
    /// <summary>
    /// Executes the specified action with retry logic and returns detailed telemetry.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A result object containing success status, attempt count, and duration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    //==============================================================================================
    public RetryResult ExecuteWithResult(Action action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        return RetryExecutor.ExecuteWithResult(this, action, cancellationToken);
    }

    //==============================================================================================
    /// <summary>
    /// Executes the specified function with retry logic and returns detailed telemetry.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A result object containing success status, attempt count, duration, and value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when func is null.</exception>
    //==============================================================================================
    public RetryResult<T> ExecuteWithResult<T>(Func<T> func, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);

        return RetryExecutor.ExecuteWithResult(this, func, cancellationToken);
    }

    //==============================================================================================
    /// <summary>
    /// Executes the specified asynchronous function with retry logic and returns detailed telemetry.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task containing result object with success status, attempt count, and duration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when func is null.</exception>
    //==============================================================================================
    public Task<RetryResult> ExecuteWithResultAsync(Func<Task> func, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);

        return RetryExecutor.ExecuteWithResultAsync(this, func, cancellationToken);
    }

    //==============================================================================================
    /// <summary>
    /// Executes the specified asynchronous function with retry logic and returns detailed telemetry.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task containing result object with success status, attempt count, duration, and value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when func is null.</exception>
    //==============================================================================================
    public Task<RetryResult<T>> ExecuteWithResultAsync<T>(Func<Task<T>> func, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);

        return RetryExecutor.ExecuteWithResultAsync(this, func, cancellationToken);
    }

    //==============================================================================================
    /// <summary>
    /// Executes the specified function with retry logic, returning a fallback value on failure.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="fallbackValue">The value to return if all retries fail.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The function result on success, or the fallback value on failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when func is null.</exception>
    //==============================================================================================
    public T Fallback<T>(Func<T> func, T fallbackValue, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            return Execute(func, cancellationToken);
        }
        catch
        {
            return fallbackValue;
        }
    }

    //==============================================================================================
    /// <summary>
    /// Executes the specified function with retry logic, using a fallback provider on failure.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="fallbackProvider">The function that provides a fallback value on failure.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The function result on success, or the fallback provider result on failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when func or fallbackProvider is null.</exception>
    //==============================================================================================
    public T Fallback<T>(Func<T> func, Func<T> fallbackProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);
        ArgumentNullException.ThrowIfNull(fallbackProvider);

        try
        {
            return Execute(func, cancellationToken);
        }
        catch
        {
            return fallbackProvider();
        }
    }

    //==============================================================================================
    /// <summary>
    /// Executes the specified asynchronous function with retry logic, returning a fallback value on failure.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="fallbackValue">The value to return if all retries fail.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task containing the function result on success, or the fallback value on failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when func is null.</exception>
    //==============================================================================================
    public async Task<T> FallbackAsync<T>(Func<Task<T>> func, T fallbackValue, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            return await ExecuteAsync(func, cancellationToken);
        }
        catch
        {
            return fallbackValue;
        }
    }

    //==============================================================================================
    /// <summary>
    /// Executes the specified asynchronous function with retry logic, using a fallback provider on failure.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="fallbackProvider">The asynchronous function that provides a fallback value on failure.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task containing the function result on success, or the fallback provider result on failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when func or fallbackProvider is null.</exception>
    //==============================================================================================
    public async Task<T> FallbackAsync<T>(Func<Task<T>> func, Func<Task<T>> fallbackProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);
        ArgumentNullException.ThrowIfNull(fallbackProvider);

        try
        {
            return await ExecuteAsync(func, cancellationToken);
        }
        catch
        {
            return await fallbackProvider();
        }
    }

    #endregion
}
