//==================================================================================================
// Tests for timeout functionality with cancellation and time limit enforcement.
// Verifies timeout behavior for both sync and async operations.
//==================================================================================================
using System.Diagnostics;

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for timeout functionality.
/// </summary>
//==================================================================================================
public sealed class TimeoutTests
{
    //==============================================================================================
    /// <summary>
    /// Tests that timeout cancels operation that takes too long.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task Timeout_should_cancel_long_running_operation()
    {
        var policy = RetryPolicy.Timeout(TimeSpan.FromMilliseconds(100));
        var stopwatch = Stopwatch.StartNew();

        var act = async () => await policy.ExecuteAsync(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        });

        await act.Should().ThrowAsync<OperationCanceledException>();
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that timeout allows fast operation to complete.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task Timeout_should_allow_fast_operation()
    {
        var policy = RetryPolicy.Timeout(TimeSpan.FromSeconds(1));

        var result = await policy.ExecuteAsync(async () =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            return 42;
        });

        result.Should().Be(42);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that negative or zero timeout throws ArgumentOutOfRangeException.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Timeout_with_negative_duration_should_throw()
    {
        var act = () => RetryPolicy.Timeout(TimeSpan.FromMilliseconds(-1));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    //==============================================================================================
    /// <summary>
    /// Tests that zero timeout throws ArgumentOutOfRangeException.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Timeout_with_zero_duration_should_throw()
    {
        var act = () => RetryPolicy.Timeout(TimeSpan.Zero);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    //==============================================================================================
    /// <summary>
    /// Tests that WithTimeout extension adds timeout to retry policy.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task WithTimeout_should_add_timeout_to_retry_policy()
    {
        var policy = RetryPolicy.Retry(3).WithTimeout(TimeSpan.FromMilliseconds(100));

        var act = async () => await policy.ExecuteAsync(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        });

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    //==============================================================================================
    /// <summary>
    /// Tests that WithTimeout with negative timeout throws.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WithTimeout_with_negative_duration_should_throw()
    {
        var policy = RetryPolicy.Retry(3);
        var act = () => policy.WithTimeout(TimeSpan.FromMilliseconds(-1));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    //==============================================================================================
    /// <summary>
    /// Tests that WithTimeout with zero timeout throws.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WithTimeout_with_zero_duration_should_throw()
    {
        var policy = RetryPolicy.Retry(3);
        var act = () => policy.WithTimeout(TimeSpan.Zero);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
