//==================================================================================================
// Specialized retry policy with custom delay sequence.
// Allows fine-grained control over delay durations between retry attempts.
//==================================================================================================

namespace Fox.RetryKit;

//==================================================================================================
/// <summary>
/// Retry policy with custom delay durations for each retry attempt.
/// </summary>
//==================================================================================================
public sealed class CustomDelayRetryPolicy : RetryPolicyBase
{
    #region Properties

    //==============================================================================================
    /// <summary>
    /// Gets the sequence of custom delay durations.
    /// </summary>
    //==============================================================================================
    public IReadOnlyList<TimeSpan> CustomDelays { get; }

    #endregion

    #region Constructors

    //==============================================================================================
    /// <summary>
    /// Initializes a new instance with custom delay sequence.
    /// </summary>
    /// <param name="delays">The sequence of delay durations for each retry attempt.</param>
    //==============================================================================================
    internal CustomDelayRetryPolicy(IReadOnlyList<TimeSpan> delays)
        : base(delays.Count, DelayStrategy.Fixed, TimeSpan.Zero, null, [], null, false, null)
    {
        CustomDelays = delays;
    }

    //==============================================================================================
    /// <summary>
    /// Initializes a new instance with custom delay sequence and additional configuration.
    /// </summary>
    /// <param name="delays">The sequence of delay durations for each retry attempt.</param>
    /// <param name="timeout">The optional timeout duration.</param>
    /// <param name="handledExceptions">The collection of exception types to handle.</param>
    /// <param name="onRetryCallback">The optional callback invoked before each retry.</param>
    //==============================================================================================
    internal CustomDelayRetryPolicy(IReadOnlyList<TimeSpan> delays, TimeSpan? timeout, IReadOnlyCollection<Type> handledExceptions, Action<Exception, int, TimeSpan>? onRetryCallback)
        : base(delays.Count, DelayStrategy.Fixed, TimeSpan.Zero, timeout, handledExceptions, onRetryCallback, false, null)
    {
        CustomDelays = delays;
    }

    //==============================================================================================
    /// <summary>
    /// Initializes a new instance with custom delay sequence and full configuration including jitter and max delay.
    /// </summary>
    /// <param name="delays">The sequence of delay durations for each retry attempt.</param>
    /// <param name="timeout">The optional timeout duration.</param>
    /// <param name="handledExceptions">The collection of exception types to handle.</param>
    /// <param name="onRetryCallback">The optional callback invoked before each retry.</param>
    /// <param name="useJitter">Whether to apply jitter to delays.</param>
    /// <param name="maxDelay">The optional maximum delay cap.</param>
    //==============================================================================================
    internal CustomDelayRetryPolicy(IReadOnlyList<TimeSpan> delays, TimeSpan? timeout, IReadOnlyCollection<Type> handledExceptions, Action<Exception, int, TimeSpan>? onRetryCallback, bool useJitter, TimeSpan? maxDelay)
        : base(delays.Count, DelayStrategy.Fixed, TimeSpan.Zero, timeout, handledExceptions, onRetryCallback, useJitter, maxDelay)
    {
        CustomDelays = delays;
    }

    #endregion

    #region Public Methods

    //==============================================================================================
    /// <summary>
    /// Configures the policy to retry only when the specified exception type is thrown.
    /// </summary>
    /// <typeparam name="TException">The exception type to handle.</typeparam>
    /// <returns>A new custom delay retry policy that handles the specified exception type.</returns>
    //==============================================================================================
    public CustomDelayRetryPolicy Handle<TException>() where TException : Exception
    {
        var exceptions = new List<Type>(HandledExceptions) { typeof(TException) };
        return new CustomDelayRetryPolicy(CustomDelays, TimeoutDuration, exceptions, OnRetryCallback, UseJitter, MaxDelay);
    }

    //==============================================================================================
    /// <summary>
    /// Configures a callback to be invoked before each retry attempt.
    /// </summary>
    /// <param name="callback">The callback action that receives exception, attempt number, and delay.</param>
    /// <returns>A new custom delay retry policy with the callback configured.</returns>
    //==============================================================================================
    public override CustomDelayRetryPolicy OnRetry(Action<Exception, int, TimeSpan> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        return new CustomDelayRetryPolicy(CustomDelays, TimeoutDuration, HandledExceptions, callback, UseJitter, MaxDelay);
    }

    //==============================================================================================
    /// <summary>
    /// Enables jitter for retry delays.
    /// </summary>
    /// <returns>A new custom delay retry policy with jitter enabled.</returns>
    //==============================================================================================
    public override CustomDelayRetryPolicy WithJitter()
    {
        return new CustomDelayRetryPolicy(CustomDelays, TimeoutDuration, HandledExceptions, OnRetryCallback, true, MaxDelay);
    }

    //==============================================================================================
    /// <summary>
    /// Sets a maximum delay cap.
    /// </summary>
    /// <param name="maxDelay">The maximum delay duration.</param>
    /// <returns>A new custom delay retry policy with maximum delay configured.</returns>
    //==============================================================================================
    public override CustomDelayRetryPolicy WithMaxDelay(TimeSpan maxDelay)
    {
        if (maxDelay <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDelay), "Maximum delay must be positive.");
        }

        return new CustomDelayRetryPolicy(CustomDelays, TimeoutDuration, HandledExceptions, OnRetryCallback, UseJitter, maxDelay);
    }

    #endregion

    #region Internal Methods

    //==============================================================================================
    /// <summary>
    /// Gets the delay for a specific retry attempt.
    /// </summary>
    /// <param name="attempt">The attempt number (1-based).</param>
    /// <returns>The delay duration for the specified attempt, or zero if out of range.</returns>
    //==============================================================================================
    internal TimeSpan GetDelayForAttempt(int attempt)
    {
        if (attempt > 0 && attempt <= CustomDelays.Count)
        {
            return CustomDelays[attempt - 1];
        }

        return TimeSpan.Zero;
    }

    #endregion
}
