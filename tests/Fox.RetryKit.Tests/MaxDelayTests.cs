//==================================================================================================
// Unit tests for maximum delay cap feature.
// Verifies delay cap enforcement with exponential backoff.
//==================================================================================================

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for maximum delay cap functionality.
/// </summary>
//==================================================================================================
public sealed class MaxDelayTests
{
    //==============================================================================================
    /// <summary>
    /// WithMaxDelay should cap exponential backoff delays.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WithMaxDelay_should_cap_exponential_delays()
    {
        var delays = new List<TimeSpan>();
        var maxDelay = TimeSpan.FromMilliseconds(500);
        var policy = RetryPolicy.ExponentialBackoff(5, TimeSpan.FromMilliseconds(100)).WithMaxDelay(maxDelay).OnRetry((_, _, delay) => delays.Add(delay));

        try
        {
            policy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        delays.Should().OnlyContain(d => d <= maxDelay);
        delays[3].Should().Be(maxDelay);
        delays[4].Should().Be(maxDelay);
    }

    //==============================================================================================
    /// <summary>
    /// WithMaxDelay should not affect delays below cap.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WithMaxDelay_should_not_affect_delays_below_cap()
    {
        var delays = new List<TimeSpan>();
        var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(100)).WithMaxDelay(TimeSpan.FromMilliseconds(500)).OnRetry((_, _, delay) => delays.Add(delay));

        try
        {
            policy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        delays.Should().OnlyContain(d => d == TimeSpan.FromMilliseconds(100));
    }

    //==============================================================================================
    /// <summary>
    /// WithMaxDelay should throw when maxDelay is zero.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WithMaxDelay_should_throw_when_zero()
    {
        var policy = RetryPolicy.Retry(3);

        var act = () => policy.WithMaxDelay(TimeSpan.Zero);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    //==============================================================================================
    /// <summary>
    /// WithMaxDelay should throw when maxDelay is negative.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WithMaxDelay_should_throw_when_negative()
    {
        var policy = RetryPolicy.Retry(3);

        var act = () => policy.WithMaxDelay(TimeSpan.FromMilliseconds(-100));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    //==============================================================================================
    /// <summary>
    /// WithMaxDelay should work asynchronously.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task WithMaxDelay_should_work_with_async_execution()
    {
        var delays = new List<TimeSpan>();
        var maxDelay = TimeSpan.FromMilliseconds(300);
        var policy = RetryPolicy.ExponentialBackoff(5, TimeSpan.FromMilliseconds(100)).WithMaxDelay(maxDelay).OnRetry((_, _, delay) => delays.Add(delay));

        try
        {
            await policy.ExecuteAsync(async () =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Test");
            });
        }
        catch
        {
        }

        delays.Should().OnlyContain(d => d <= maxDelay);
    }

    //==============================================================================================
    /// <summary>
    /// WithMaxDelay should work with WaitAndRetry.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WithMaxDelay_should_work_with_custom_delays()
    {
        var delays = new List<TimeSpan>();
        var customDelays = new[] { TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(600), TimeSpan.FromMilliseconds(800) };
        var policy = RetryPolicy.WaitAndRetry(customDelays).WithMaxDelay(TimeSpan.FromMilliseconds(500)).OnRetry((_, _, delay) => delays.Add(delay));

        try
        {
            policy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        delays.Should().HaveCount(3);
        delays[0].Should().Be(TimeSpan.FromMilliseconds(100));
        delays[1].Should().Be(TimeSpan.FromMilliseconds(500));
        delays[2].Should().Be(TimeSpan.FromMilliseconds(500));
    }
}
