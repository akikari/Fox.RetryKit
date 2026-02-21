//==================================================================================================
// Unit tests for WaitAndRetry feature with custom delay sequences.
// Verifies custom delay configuration and execution timing.
//==================================================================================================

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for WaitAndRetry custom delay functionality.
/// </summary>
//==================================================================================================
public sealed class WaitAndRetryTests
{
    //==============================================================================================
    /// <summary>
    /// WaitAndRetry should use custom delay sequence.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WaitAndRetry_should_use_custom_delays()
    {
        var actualDelays = new List<TimeSpan>();
        var customDelays = new[] { TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(300) };
        var policy = RetryPolicy.WaitAndRetry(customDelays).OnRetry((_, _, delay) => actualDelays.Add(delay));

        try
        {
            policy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        actualDelays.Should().Equal(customDelays);
    }

    //==============================================================================================
    /// <summary>
    /// WaitAndRetry should respect the number of retries from delay count.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WaitAndRetry_should_set_retry_count_from_delays()
    {
        var attempts = 0;
        var policy = RetryPolicy.WaitAndRetry([TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(20)]);

        try
        {
            policy.Execute(() =>
            {
                attempts++;
                throw new InvalidOperationException("Test");
            });
        }
        catch
        {
        }

        attempts.Should().Be(3);
    }

    //==============================================================================================
    /// <summary>
    /// WaitAndRetry should throw when delays is null.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WaitAndRetry_should_throw_when_delays_is_null()
    {
        var act = () => RetryPolicy.WaitAndRetry(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    //==============================================================================================
    /// <summary>
    /// WaitAndRetry should throw when delays is empty.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WaitAndRetry_should_throw_when_delays_is_empty()
    {
        var act = () => RetryPolicy.WaitAndRetry([]);

        act.Should().Throw<ArgumentException>();
    }

    //==============================================================================================
    /// <summary>
    /// WaitAndRetry should work asynchronously.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task WaitAndRetry_should_work_with_async_execution()
    {
        var actualDelays = new List<TimeSpan>();
        var customDelays = new[] { TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(150) };
        var policy = RetryPolicy.WaitAndRetry(customDelays).OnRetry((_, _, delay) => actualDelays.Add(delay));

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

        actualDelays.Should().Equal(customDelays);
    }

    //==============================================================================================
    /// <summary>
    /// WaitAndRetry should combine with exception filtering.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WaitAndRetry_should_combine_with_exception_filtering()
    {
        var attempts = 0;
        var policy = RetryPolicy.WaitAndRetry([TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(20)]).Handle<InvalidOperationException>();

        try
        {
            policy.Execute(() =>
            {
                attempts++;

                if (attempts == 1)
                {
                    throw new InvalidOperationException("Handled");
                }

                throw new ArgumentException("Unhandled");
            });
        }
        catch (ArgumentException)
        {
        }

        attempts.Should().Be(2);
    }

    //==============================================================================================
    /// <summary>
    /// WaitAndRetry should use zero delay for attempts beyond sequence.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void WaitAndRetry_should_use_zero_delay_beyond_sequence()
    {
        var delays = new List<TimeSpan>();
        var customDelays = new[] { TimeSpan.FromMilliseconds(100) };
        var policy = RetryPolicy.WaitAndRetry(customDelays).OnRetry((_, _, delay) => delays.Add(delay));

        try
        {
            policy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        delays.Should().HaveCount(1);
        delays[0].Should().Be(TimeSpan.FromMilliseconds(100));
    }
}
