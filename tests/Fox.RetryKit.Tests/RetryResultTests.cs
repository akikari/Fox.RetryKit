//==================================================================================================
// Unit tests for RetryResult telemetry feature.
// Verifies detailed telemetry collection for retry operations.
//==================================================================================================

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for RetryResult telemetry functionality.
/// </summary>
//==================================================================================================
public sealed class RetryResultTests
{
    //==============================================================================================
    /// <summary>
    /// ExecuteWithResult should return success result on first attempt.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExecuteWithResult_should_return_success_on_first_attempt()
    {
        var policy = RetryPolicy.Retry(3);

        var result = policy.ExecuteWithResult(() => 42);

        result.Success.Should().BeTrue();
        result.Value.Should().Be(42);
        result.Attempts.Should().Be(1);
        result.LastException.Should().BeNull();
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResult should return success after retries.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExecuteWithResult_should_return_success_after_retries()
    {
        var attempts = 0;
        var policy = RetryPolicy.Retry(3);

        var result = policy.ExecuteWithResult(() =>
        {
            attempts++;

            if (attempts < 3)
            {
                throw new InvalidOperationException("Fail");
            }

            return "success";
        });

        result.Success.Should().BeTrue();
        result.Value.Should().Be("success");
        result.Attempts.Should().Be(3);
        result.LastException.Should().BeNull();
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResult should return failure result when all retries exhausted.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExecuteWithResult_should_return_failure_on_exhaustion()
    {
        var policy = RetryPolicy.Retry(2);

        var result = policy.ExecuteWithResult<string>(() => throw new InvalidOperationException("Always fails"));

        result.Success.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Attempts.Should().Be(3);
        result.LastException.Should().BeOfType<InvalidOperationException>();
        result.LastException!.Message.Should().Be("Always fails");
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResult should track total duration including delays.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExecuteWithResult_should_track_total_duration()
    {
        var policy = RetryPolicy.Retry(2, TimeSpan.FromMilliseconds(100));
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var result = policy.ExecuteWithResult<string>(() => throw new InvalidOperationException("Fail"));

        sw.Stop();
        result.TotalDuration.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(200));
        result.TotalDuration.Should().BeLessThanOrEqualTo(sw.Elapsed);
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResult for Action should return success without value.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExecuteWithResult_for_action_should_return_success()
    {
        var policy = RetryPolicy.Retry(2);
        var executed = false;

        var result = policy.ExecuteWithResult(() => executed = true);

        result.Success.Should().BeTrue();
        result.Attempts.Should().Be(1);
        result.LastException.Should().BeNull();
        executed.Should().BeTrue();
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResultAsync should return success result on first attempt.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteWithResultAsync_should_return_success_on_first_attempt()
    {
        var policy = RetryPolicy.Retry(3);

        var result = await policy.ExecuteWithResultAsync(async () =>
        {
            await Task.Yield();
            return 42;
        });

        result.Success.Should().BeTrue();
        result.Value.Should().Be(42);
        result.Attempts.Should().Be(1);
        result.LastException.Should().BeNull();
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResultAsync should return success after retries.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteWithResultAsync_should_return_success_after_retries()
    {
        var attempts = 0;
        var policy = RetryPolicy.Retry(3);

        var result = await policy.ExecuteWithResultAsync(async () =>
        {
            attempts++;
            await Task.Yield();

            if (attempts < 3)
            {
                throw new InvalidOperationException("Fail");
            }

            return "success";
        });

        result.Success.Should().BeTrue();
        result.Value.Should().Be("success");
        result.Attempts.Should().Be(3);
        result.LastException.Should().BeNull();
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResultAsync should return failure result when all retries exhausted.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteWithResultAsync_should_return_failure_on_exhaustion()
    {
        var policy = RetryPolicy.Retry(2);

        var result = await policy.ExecuteWithResultAsync<string>(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("Always fails");
        });

        result.Success.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Attempts.Should().Be(3);
        result.LastException.Should().BeOfType<InvalidOperationException>();
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResultAsync should track total duration including delays.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteWithResultAsync_should_track_total_duration()
    {
        var policy = RetryPolicy.Retry(2, TimeSpan.FromMilliseconds(100));
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var result = await policy.ExecuteWithResultAsync<string>(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("Fail");
        });

        sw.Stop();
        result.TotalDuration.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(200));
        result.TotalDuration.Should().BeLessThanOrEqualTo(sw.Elapsed);
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResultAsync for Task should return success without value.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteWithResultAsync_for_task_should_return_success()
    {
        var policy = RetryPolicy.Retry(2);
        var executed = false;

        var result = await policy.ExecuteWithResultAsync(async () =>
        {
            await Task.Yield();
            executed = true;
        });

        result.Success.Should().BeTrue();
        result.Attempts.Should().Be(1);
        result.LastException.Should().BeNull();
        executed.Should().BeTrue();
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResult should work with exception filtering.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExecuteWithResult_should_work_with_exception_filtering()
    {
        var attempts = 0;
        var policy = RetryPolicy.Retry(3).Handle<InvalidOperationException>();

        var result = policy.ExecuteWithResult<int>(() =>
        {
            attempts++;

            if (attempts == 1)
            {
                throw new InvalidOperationException("Handled");
            }

            if (attempts == 2)
            {
                throw new ArgumentException("Unhandled");
            }

            return 42;
        });

        attempts.Should().Be(2);
        result.Success.Should().BeFalse();
        result.LastException.Should().BeOfType<ArgumentException>();
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResult should capture timeout exceptions.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteWithResultAsync_should_capture_timeout()
    {
        var policy = RetryPolicy.Timeout(TimeSpan.FromMilliseconds(100));

        var result = await policy.ExecuteWithResultAsync(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            return "never";
        });

        result.Success.Should().BeFalse();
        result.Value.Should().BeNull();
        result.LastException.Should().BeOfType<OperationCanceledException>();
    }
}
