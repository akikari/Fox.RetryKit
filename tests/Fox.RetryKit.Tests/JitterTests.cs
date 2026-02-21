//==================================================================================================
// Unit tests for jitter feature.
// Verifies random variation in retry delays to prevent thundering herd.
//==================================================================================================

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for jitter functionality.
/// </summary>
//==================================================================================================
public sealed class JitterTests
{
    //==============================================================================================
    /// <summary>
    /// Jitter should apply random variation to delays.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WithJitter_should_vary_delay_durations()
    {
        var delays = new List<TimeSpan>();
        var policy = RetryPolicy.Retry(10, TimeSpan.FromMilliseconds(100)).WithJitter().OnRetry((_, _, delay) => delays.Add(delay));

        try
        {
            policy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        delays.Should().HaveCount(10);
        delays.Should().OnlyContain(d => d >= TimeSpan.FromMilliseconds(75) && d <= TimeSpan.FromMilliseconds(125));
        delays.Distinct().Should().HaveCountGreaterThan(1);
    }

    //==============================================================================================
    /// <summary>
    /// Jitter should work with exponential backoff.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WithJitter_should_work_with_exponential_backoff()
    {
        var delays = new List<TimeSpan>();
        var policy = RetryPolicy.ExponentialBackoff(5, TimeSpan.FromMilliseconds(100)).WithJitter().OnRetry((_, _, delay) => delays.Add(delay));

        try
        {
            policy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        delays.Should().HaveCount(5);
        delays[0].Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(75)).And.BeLessThanOrEqualTo(TimeSpan.FromMilliseconds(125));
        delays[1].Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(150)).And.BeLessThanOrEqualTo(TimeSpan.FromMilliseconds(250));
    }

    //==============================================================================================
    /// <summary>
    /// Jitter should not be applied when delay is zero.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WithJitter_should_not_affect_zero_delays()
    {
        var delays = new List<TimeSpan>();
        var policy = RetryPolicy.Retry(3).WithJitter().OnRetry((_, _, delay) => delays.Add(delay));

        try
        {
            policy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        delays.Should().OnlyContain(d => d == TimeSpan.Zero);
    }

    //==============================================================================================
    /// <summary>
    /// Jitter should work asynchronously.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task WithJitter_should_work_with_async_execution()
    {
        var delays = new List<TimeSpan>();
        var policy = RetryPolicy.Retry(5, TimeSpan.FromMilliseconds(100)).WithJitter().OnRetry((_, _, delay) => delays.Add(delay));

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

        delays.Should().HaveCount(5);
        delays.Should().OnlyContain(d => d >= TimeSpan.FromMilliseconds(75) && d <= TimeSpan.FromMilliseconds(125));
    }

    //==============================================================================================
    /// <summary>
    /// Jitter should apply ±25% random variation.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WithJitter_should_apply_25_percent_variation()
    {
        var delays = new List<TimeSpan>();
        var baseDelay = TimeSpan.FromMilliseconds(1000);
        var policy = RetryPolicy.Retry(100, baseDelay).WithJitter().OnRetry((_, _, delay) => delays.Add(delay));

        try
        {
            policy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        var minDelay = TimeSpan.FromMilliseconds(750);
        var maxDelay = TimeSpan.FromMilliseconds(1250);

        delays.Should().OnlyContain(d => d >= minDelay && d <= maxDelay);
        delays.Min().Should().BeCloseTo(minDelay, TimeSpan.FromMilliseconds(100));
        delays.Max().Should().BeCloseTo(maxDelay, TimeSpan.FromMilliseconds(100));
    }

    //==============================================================================================
    /// <summary>
    /// Jitter should combine with MaxDelay.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WithJitter_should_combine_with_max_delay()
    {
        var delays = new List<TimeSpan>();
        var policy = RetryPolicy.ExponentialBackoff(10, TimeSpan.FromMilliseconds(100)).WithJitter().WithMaxDelay(TimeSpan.FromMilliseconds(300)).OnRetry((_, _, delay) => delays.Add(delay));

        try
        {
            policy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        delays.Should().OnlyContain(d => d <= TimeSpan.FromMilliseconds(375));
    }
}
