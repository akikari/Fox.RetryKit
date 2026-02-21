//==================================================================================================
// Represents the result of a retry operation without a return value and telemetry information.
// Provides metrics about retry attempts, duration, and outcome.
//==================================================================================================

namespace Fox.RetryKit;

//==================================================================================================
/// <summary>
/// Represents the result of a retry operation without a return value.
/// </summary>
//==================================================================================================
public sealed class RetryResult
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

    internal RetryResult(bool success, int attempts, TimeSpan totalDuration, Exception? lastException)
    {
        Success = success;
        Attempts = attempts;
        TotalDuration = totalDuration;
        LastException = lastException;
    }

    #endregion
}
