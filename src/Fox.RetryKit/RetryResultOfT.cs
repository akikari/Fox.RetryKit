//==================================================================================================
// Represents the result of a retry operation with a return value and telemetry information.
// Provides metrics about retry attempts, duration, outcome, and the result value.
//==================================================================================================

namespace Fox.RetryKit;

//==================================================================================================
/// <summary>
/// Represents the result of a retry operation with telemetry and metrics.
/// </summary>
/// <typeparam name="T">The type of the operation result.</typeparam>
//==================================================================================================
public sealed class RetryResult<T>
{
    #region Properties

    //==============================================================================================
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    //==============================================================================================
    public bool Success { get; }

    //==============================================================================================
    /// <summary>
    /// Gets the result value if the operation succeeded.
    /// </summary>
    //==============================================================================================
    public T? Value { get; }

    //==============================================================================================
    /// <summary>
    /// Gets the number of attempts made (including the initial attempt).
    /// </summary>
    //==============================================================================================
    public int Attempts { get; }

    //==============================================================================================
    /// <summary>
    /// Gets the total duration of all attempts including delays.
    /// </summary>
    //==============================================================================================
    public TimeSpan TotalDuration { get; }

    //==============================================================================================
    /// <summary>
    /// Gets the last exception thrown, if any.
    /// </summary>
    //==============================================================================================
    public Exception? LastException { get; }

    #endregion

    #region Constructors

    internal RetryResult(bool success, T? value, int attempts, TimeSpan totalDuration, Exception? lastException)
    {
        Success = success;
        Value = value;
        Attempts = attempts;
        TotalDuration = totalDuration;
        LastException = lastException;
    }

    #endregion
}
