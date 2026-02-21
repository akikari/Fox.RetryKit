//==================================================================================================
// Internal policy class for conditional retry based on operation result.
// Allows retry decisions based on the return value using a predicate function.
//==================================================================================================

namespace Fox.RetryKit;

//==================================================================================================
/// <summary>
/// Internal retry policy that supports conditional retry based on operation result.
/// </summary>
//==================================================================================================
public sealed class RetryIfPolicy<T> : RetryPolicyBase
{
    #region Properties

    //==============================================================================================
    /// <summary>
    /// Gets the predicate that determines whether to retry based on the result.
    /// </summary>
    //==============================================================================================
    public Func<T, bool> RetryPredicate { get; }

    #endregion

    #region Constructors

    //==============================================================================================
    /// <summary>
    /// Initializes a new instance with conditional retry based on result predicate.
    /// </summary>
    /// <param name="retryCount">The maximum number of retry attempts.</param>
    /// <param name="strategy">The delay strategy to use.</param>
    /// <param name="delay">The base delay duration.</param>
    /// <param name="timeout">The optional timeout duration.</param>
    /// <param name="handledExceptions">The collection of exception types to handle.</param>
    /// <param name="onRetryCallback">The optional callback invoked before each retry.</param>
    /// <param name="useJitter">Whether to apply jitter to delays.</param>
    /// <param name="maxDelay">The optional maximum delay cap.</param>
    /// <param name="retryPredicate">The predicate that determines whether to retry based on result.</param>
    //==============================================================================================
    internal RetryIfPolicy(int retryCount, DelayStrategy strategy, TimeSpan delay, TimeSpan? timeout, IReadOnlyCollection<Type> handledExceptions, Action<Exception, int, TimeSpan>? onRetryCallback, bool useJitter, TimeSpan? maxDelay, Func<T, bool> retryPredicate)
        : base(retryCount, strategy, delay, timeout, handledExceptions, onRetryCallback, useJitter, maxDelay)
    {
        RetryPredicate = retryPredicate ?? throw new ArgumentNullException(nameof(retryPredicate));
    }

    #endregion

    #region Public Methods

    //==============================================================================================
    /// <summary>
    /// Configures a callback to be invoked before each retry attempt.
    /// </summary>
    /// <param name="callback">The callback action that receives exception, attempt number, and delay.</param>
    /// <returns>A new retry-if policy with the callback configured.</returns>
    //==============================================================================================
    public override RetryIfPolicy<T> OnRetry(Action<Exception, int, TimeSpan> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        return new RetryIfPolicy<T>(RetryCount, Strategy, Delay, TimeoutDuration, HandledExceptions, callback, UseJitter, MaxDelay, RetryPredicate);
    }

    //==============================================================================================
    /// <summary>
    /// Enables jitter for retry delays.
    /// </summary>
    /// <returns>A new retry-if policy with jitter enabled.</returns>
    //==============================================================================================
    public override RetryIfPolicy<T> WithJitter()
    {
        return new RetryIfPolicy<T>(RetryCount, Strategy, Delay, TimeoutDuration, HandledExceptions, OnRetryCallback, true, MaxDelay, RetryPredicate);
    }

    //==============================================================================================
    /// <summary>
    /// Sets a maximum delay cap.
    /// </summary>
    /// <param name="maxDelay">The maximum delay duration.</param>
    /// <returns>A new retry-if policy with maximum delay configured.</returns>
    //==============================================================================================
    public override RetryIfPolicy<T> WithMaxDelay(TimeSpan maxDelay)
    {
        if (maxDelay <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDelay), "Maximum delay must be positive.");
        }

        return new RetryIfPolicy<T>(RetryCount, Strategy, Delay, TimeoutDuration, HandledExceptions, OnRetryCallback, UseJitter, maxDelay, RetryPredicate);
    }

    //==============================================================================================
    /// <summary>
    /// Executes the specified function with conditional retry logic.
    /// </summary>
    /// <param name="func">The function to execute.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The result of the function execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when func is null.</exception>
    //==============================================================================================
    public T Execute(Func<T> func, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);

        return RetryExecutor.Execute(this, func, cancellationToken);
    }

    //==============================================================================================
    /// <summary>
    /// Executes the specified asynchronous function with conditional retry logic.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when func is null.</exception>
    //==============================================================================================
    public Task<T> ExecuteAsync(Func<Task<T>> func, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);

        return RetryExecutor.ExecuteAsync(this, func, cancellationToken);
    }

    #endregion
}
