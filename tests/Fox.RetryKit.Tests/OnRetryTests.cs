//==================================================================================================
// Unit tests for OnRetry callback feature.
// Verifies callback invocation, parameters, and integration with retry logic.
//==================================================================================================

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for OnRetry callback functionality.
/// </summary>
//==================================================================================================
public sealed class OnRetryTests
{
    //==============================================================================================
    /// <summary>
    /// OnRetry callback should be invoked before each retry attempt.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void OnRetry_should_be_invoked_before_each_retry()
    {
        var invocations = new List<(Exception, int, TimeSpan)>();
        var policy = RetryPolicy.Retry(3).OnRetry((ex, attempt, delay) => invocations.Add((ex, attempt, delay)));

        var callCount = 0;

        try
        {
            policy.Execute(() =>
            {
                callCount++;

                if (callCount < 3)
                {
                    throw new InvalidOperationException($"Failure {callCount}");
                }
            });
        }
        catch
        {
        }

        invocations.Should().HaveCount(2);
        invocations[0].Item2.Should().Be(1);
        invocations[1].Item2.Should().Be(2);
    }

    //==============================================================================================
    /// <summary>
    /// OnRetry callback should receive correct exception instance.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void OnRetry_should_receive_correct_exception()
    {
        Exception? capturedEx = null;
        var policy = RetryPolicy.Retry(2).OnRetry((ex, _, _) => capturedEx = ex);

        try
        {
            policy.Execute(() => throw new InvalidOperationException("Test error"));
        }
        catch
        {
        }

        capturedEx.Should().NotBeNull();
        capturedEx.Should().BeOfType<InvalidOperationException>();
        capturedEx!.Message.Should().Be("Test error");
    }

    //==============================================================================================
    /// <summary>
    /// OnRetry callback should receive correct delay duration.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void OnRetry_should_receive_correct_delay()
    {
        var delays = new List<TimeSpan>();
        var policy = RetryPolicy.Retry(2, TimeSpan.FromMilliseconds(100)).OnRetry((_, _, delay) => delays.Add(delay));

        try
        {
            policy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        delays.Should().HaveCount(2);
        delays[0].Should().Be(TimeSpan.FromMilliseconds(100));
        delays[1].Should().Be(TimeSpan.FromMilliseconds(100));
    }

    //==============================================================================================
    /// <summary>
    /// OnRetry callback should work with exponential backoff.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void OnRetry_should_work_with_exponential_backoff()
    {
        var attempts = new List<int>();
        var policy = RetryPolicy.ExponentialBackoff(3, TimeSpan.FromMilliseconds(100)).OnRetry((_, attempt, _) => attempts.Add(attempt));

        try
        {
            policy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        attempts.Should().Equal(1, 2, 3);
    }

    //==============================================================================================
    /// <summary>
    /// OnRetry callback should work asynchronously.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task OnRetry_should_work_with_async_execution()
    {
        var invocations = 0;
        var policy = RetryPolicy.Retry(3).OnRetry((_, _, _) => invocations++);

        var callCount = 0;

        try
        {
            await policy.ExecuteAsync(async () =>
            {
                callCount++;
                await Task.Yield();

                if (callCount < 3)
                {
                    throw new InvalidOperationException("Failure");
                }
            });
        }
        catch
        {
        }

        invocations.Should().Be(2);
    }

    //==============================================================================================
    /// <summary>
    /// OnRetry callback should not be invoked on final failure.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void OnRetry_should_not_be_invoked_on_final_failure()
    {
        var invocations = 0;
        var policy = RetryPolicy.Retry(2).OnRetry((_, _, _) => invocations++);

        try
        {
            policy.Execute(() => throw new InvalidOperationException("Always fails"));
        }
        catch
        {
        }

        invocations.Should().Be(2);
    }

    //==============================================================================================
    /// <summary>
    /// OnRetry callback should not be invoked when operation succeeds on first attempt.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void OnRetry_should_not_be_invoked_on_immediate_success()
    {
        var invocations = 0;
        var policy = RetryPolicy.Retry(3).OnRetry((_, _, _) => invocations++);

        policy.Execute(() => { });

        invocations.Should().Be(0);
    }

    //==============================================================================================
    /// <summary>
    /// OnRetry throws ArgumentNullException when callback is null.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void OnRetry_should_throw_when_callback_is_null()
    {
        var policy = RetryPolicy.Retry(3);

        var act = () => policy.OnRetry(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
