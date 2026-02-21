//==================================================================================================
// Extension methods for RetryPolicy to provide additional fluent API capabilities.
// Enables method chaining for timeout configuration.
//==================================================================================================

namespace Fox.RetryKit;

//==================================================================================================
/// <summary>
/// Extension methods for configuring RetryPolicy instances.
/// </summary>
//==================================================================================================
public static class RetryPolicyExtensions
{
    //==============================================================================================
    /// <summary>
    /// Adds a timeout constraint to the retry policy.
    /// </summary>
    /// <param name="policy">The retry policy to configure.</param>
    /// <param name="timeout">The maximum duration before timeout.</param>
    /// <returns>A new retry policy with timeout configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown when policy is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when timeout is negative or zero.</exception>
    //==============================================================================================
    public static RetryPolicy WithTimeout(this RetryPolicy policy, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout duration must be positive.");
        }

        var handledExceptions = policy.HandledExceptions;
        return new RetryPolicy(policy.RetryCount, policy.Strategy, policy.Delay, timeout, handledExceptions, policy.OnRetryCallback, policy.UseJitter, policy.MaxDelay);
    }
}
