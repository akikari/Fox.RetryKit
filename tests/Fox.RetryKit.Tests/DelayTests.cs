//==================================================================================================
// Tests for delay functionality with fixed and exponential backoff strategies.
// Verifies delay timing and exponential growth behavior.
//==================================================================================================
using System.Diagnostics;

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for delay and exponential backoff functionality.
/// </summary>
//==================================================================================================
public sealed class DelayTests
{
    //==============================================================================================
    /// <summary>
    /// Tests that fixed delay waits between retries.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Retry_with_fixed_delay_should_wait_between_attempts()
    {
        var policy = RetryPolicy.Retry(2, TimeSpan.FromMilliseconds(100));
        var attempts = 0;
        var stopwatch = Stopwatch.StartNew();

        var act = () => policy.Execute(() =>
        {
            attempts++;
            throw new InvalidOperationException("Test exception");
        });

        act.Should().Throw<InvalidOperationException>();
        stopwatch.Stop();

        attempts.Should().Be(3);
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(200);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that exponential backoff increases delay exponentially.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExponentialBackoff_should_increase_delay_exponentially()
    {
        var policy = RetryPolicy.ExponentialBackoff(3, TimeSpan.FromMilliseconds(50));
        var attempts = 0;
        var stopwatch = Stopwatch.StartNew();

        var act = () => policy.Execute(() =>
        {
            attempts++;
            throw new InvalidOperationException("Test exception");
        });

        act.Should().Throw<InvalidOperationException>();
        stopwatch.Stop();

        attempts.Should().Be(4);
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(350);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that negative delay throws ArgumentOutOfRangeException.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Retry_with_negative_delay_should_throw()
    {
        var act = () => RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(-1));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    //==============================================================================================
    /// <summary>
    /// Tests that negative initial delay for exponential backoff throws.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExponentialBackoff_with_negative_delay_should_throw()
    {
        var act = () => RetryPolicy.ExponentialBackoff(3, TimeSpan.FromMilliseconds(-1));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    //==============================================================================================
    /// <summary>
    /// Tests that negative retry count for exponential backoff throws.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExponentialBackoff_with_negative_count_should_throw()
    {
        var act = () => RetryPolicy.ExponentialBackoff(-1, TimeSpan.FromMilliseconds(100));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
