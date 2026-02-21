//==================================================================================================
// Represents a retry policy with configurable retry count, delay, and exception filtering.
// Provides a fluent builder API for creating and executing retry operations.
//==================================================================================================

namespace Fox.RetryKit;

//==================================================================================================
/// <summary>
/// Represents a retry policy that defines how operations should be retried on failure.
/// Supports configurable retry count, delay strategies, timeout, and exception filtering.
/// </summary>
//==================================================================================================
public class RetryPolicy : RetryPolicyBase
{
    #region Constructors

    //==============================================================================================
    /// <summary>
    /// Initializes a new instance of the retry policy with specified parameters.
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
    internal RetryPolicy(int retryCount, DelayStrategy strategy, TimeSpan delay, TimeSpan? timeout, IReadOnlyCollection<Type> handledExceptions, Action<Exception, int, TimeSpan>? onRetryCallback = null, bool useJitter = false, TimeSpan? maxDelay = null)
        : base(retryCount, strategy, delay, timeout, handledExceptions, onRetryCallback, useJitter, maxDelay)
    {
    }

    #endregion

    #region Public Methods

    //==============================================================================================
    /// <summary>
    /// Creates a retry policy with the specified number of retry attempts and no delay.
    /// </summary>
    /// <param name="count">The maximum number of retry attempts.</param>
    /// <returns>A new retry policy.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when count is negative.</exception>
    //==============================================================================================
    public static RetryPolicy Retry(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Retry count cannot be negative.");
        }

        return new RetryPolicy(count, DelayStrategy.Fixed, TimeSpan.Zero, null, []);
    }

    //==============================================================================================
    /// <summary>
    /// Creates a retry policy with the specified number of retry attempts and fixed delay.
    /// </summary>
    /// <param name="count">The maximum number of retry attempts.</param>
    /// <param name="delay">The delay duration between retry attempts.</param>
    /// <returns>A new retry policy.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when count is negative or delay is negative.</exception>
    //==============================================================================================
    public static RetryPolicy Retry(int count, TimeSpan delay)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Retry count cannot be negative.");
        }

        if (delay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), "Delay cannot be negative.");
        }

        return new RetryPolicy(count, DelayStrategy.Fixed, delay, null, []);
    }

    //==============================================================================================
    /// <summary>
    /// Creates a retry policy with exponential backoff delay strategy.
    /// </summary>
    /// <param name="retries">The maximum number of retry attempts.</param>
    /// <param name="initialDelay">The initial delay duration that will be exponentially increased.</param>
    /// <returns>A new retry policy with exponential backoff.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when retries is negative or initialDelay is negative.</exception>
    //==============================================================================================
    public static RetryPolicy ExponentialBackoff(int retries, TimeSpan initialDelay)
    {
        if (retries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(retries), "Retry count cannot be negative.");
        }

        if (initialDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(initialDelay), "Initial delay cannot be negative.");
        }

        return new RetryPolicy(retries, DelayStrategy.Exponential, initialDelay, null, []);
    }

    //==============================================================================================
    /// <summary>
    /// Creates a timeout policy that will cancel the operation after the specified duration.
    /// </summary>
    /// <param name="duration">The maximum duration before timeout.</param>
    /// <returns>A new retry policy with timeout.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when duration is negative or zero.</exception>
    //==============================================================================================
    public static RetryPolicy Timeout(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Timeout duration must be positive.");
        }

        return new RetryPolicy(0, DelayStrategy.Fixed, TimeSpan.Zero, duration, []);
    }

    //==============================================================================================
    /// <summary>
    /// Configures the policy to retry only when the specified exception type is thrown.
    /// </summary>
    /// <typeparam name="TException">The exception type to handle.</typeparam>
    /// <returns>A new retry policy that handles the specified exception type.</returns>
    //==============================================================================================
    public RetryPolicy Handle<TException>() where TException : Exception
    {
        var exceptions = new List<Type>(HandledExceptions) { typeof(TException) };
        return new RetryPolicy(RetryCount, Strategy, Delay, TimeoutDuration, exceptions, OnRetryCallback, UseJitter, MaxDelay);
    }

    //==============================================================================================
    /// <summary>
    /// Configures a callback to be invoked before each retry attempt.
    /// </summary>
    /// <param name="callback">The callback action that receives exception, attempt number, and delay.</param>
    /// <returns>A new retry policy with the callback configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
    //==============================================================================================
    public override RetryPolicy OnRetry(Action<Exception, int, TimeSpan> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        return new RetryPolicy(RetryCount, Strategy, Delay, TimeoutDuration, HandledExceptions, callback, UseJitter, MaxDelay);
    }

    //==============================================================================================
    /// <summary>
    /// Enables jitter (random variation) for retry delays to prevent thundering herd.
    /// Applies ±25% random variation to calculated delays.
    /// </summary>
    /// <returns>A new retry policy with jitter enabled.</returns>
    //==============================================================================================
    public override RetryPolicy WithJitter()
    {
        return new RetryPolicy(RetryCount, Strategy, Delay, TimeoutDuration, HandledExceptions, OnRetryCallback, true, MaxDelay);
    }

    //==============================================================================================
    /// <summary>
    /// Sets a maximum delay cap for exponential backoff to prevent excessively long waits.
    /// </summary>
    /// <param name="maxDelay">The maximum delay duration.</param>
    /// <returns>A new retry policy with maximum delay configured.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxDelay is negative or zero.</exception>
    //==============================================================================================
    public override RetryPolicy WithMaxDelay(TimeSpan maxDelay)
    {
        if (maxDelay <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDelay), "Maximum delay must be positive.");
        }

        return new RetryPolicy(RetryCount, Strategy, Delay, TimeoutDuration, HandledExceptions, OnRetryCallback, UseJitter, maxDelay);
    }

    //==============================================================================================
    /// <summary>
    /// Configures conditional retry based on the operation result.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="predicate">The predicate that determines whether to retry based on result.</param>
    /// <returns>A new retry policy with conditional retry configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
    //==============================================================================================
    public RetryIfPolicy<T> RetryIf<T>(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return new RetryIfPolicy<T>(RetryCount, Strategy, Delay, TimeoutDuration, HandledExceptions, OnRetryCallback, UseJitter, MaxDelay, predicate);
    }

    //==============================================================================================
    /// <summary>
    /// Creates a retry policy with custom delay durations for each retry attempt.
    /// </summary>
    /// <param name="delays">The sequence of delay durations between retry attempts.</param>
    /// <returns>A new retry policy with custom delays.</returns>
    /// <exception cref="ArgumentNullException">Thrown when delays is null.</exception>
    /// <exception cref="ArgumentException">Thrown when delays is empty.</exception>
    //==============================================================================================
    public static CustomDelayRetryPolicy WaitAndRetry(IEnumerable<TimeSpan> delays)
    {
        ArgumentNullException.ThrowIfNull(delays);

        var delayList = delays.ToList();

        if (delayList.Count == 0)
        {
            throw new ArgumentException("Delays collection cannot be empty.", nameof(delays));
        }

        return new CustomDelayRetryPolicy(delayList);
    }

    #endregion
}
