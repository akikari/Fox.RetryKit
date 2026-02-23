//==================================================================================================
// Defensive code tests for Fox.RetryKit internal members.
// Tests defensive null checks and argument validation using InternalsVisibleTo.
//==================================================================================================

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for defensive code patterns in internal members.
/// </summary>
//==================================================================================================
public sealed class DefensiveCodeTests
{
    //==============================================================================================
    /// <summary>
    /// RetryIfPolicy OnRetry should return new instance with callback.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryIfPolicy_OnRetry_should_return_new_instance()
    {
        var policy = RetryPolicy.Retry(2).RetryIf<int>(x => x < 0);
        var callbackInvoked = false;

        var newPolicy = policy.OnRetry((ex, attempt, delay) => callbackInvoked = true);

        newPolicy.Should().NotBeSameAs(policy);

        try
        {
            newPolicy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        callbackInvoked.Should().BeTrue();
    }

    //==============================================================================================
    /// <summary>
    /// RetryIfPolicy WithJitter should return new instance with jitter enabled.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryIfPolicy_WithJitter_should_return_new_instance()
    {
        var policy = RetryPolicy.Retry(2, TimeSpan.FromMilliseconds(100)).RetryIf<int>(x => x < 0);
        var delays = new List<TimeSpan>();

        var newPolicy = policy.WithJitter().OnRetry((ex, attempt, delay) => delays.Add(delay));

        newPolicy.Should().NotBeSameAs(policy);

        try
        {
            newPolicy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        delays.Should().HaveCount(2);
        delays.Should().OnlyContain(d => d >= TimeSpan.FromMilliseconds(75) && d <= TimeSpan.FromMilliseconds(125));
    }

    //==============================================================================================
    /// <summary>
    /// RetryIfPolicy WithMaxDelay should return new instance with max delay configured.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryIfPolicy_WithMaxDelay_should_return_new_instance()
    {
        var policy = RetryPolicy.ExponentialBackoff(3, TimeSpan.FromMilliseconds(100)).RetryIf<int>(x => x < 0);
        var delays = new List<TimeSpan>();

        var newPolicy = policy.WithMaxDelay(TimeSpan.FromMilliseconds(150)).OnRetry((ex, attempt, delay) => delays.Add(delay));

        newPolicy.Should().NotBeSameAs(policy);

        try
        {
            newPolicy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        delays.Should().HaveCount(3);
        delays.All(d => d <= TimeSpan.FromMilliseconds(150)).Should().BeTrue();
    }

    //==============================================================================================
    /// <summary>
    /// CustomDelayRetryPolicy WithJitter should return new instance with jitter enabled.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void CustomDelayRetryPolicy_WithJitter_should_return_new_instance()
    {
        var customDelays = new[] { TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200) };
        var policy = RetryPolicy.WaitAndRetry(customDelays);
        var delays = new List<TimeSpan>();

        var newPolicy = policy.WithJitter().OnRetry((ex, attempt, delay) => delays.Add(delay));

        newPolicy.Should().NotBeSameAs(policy);

        try
        {
            newPolicy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        delays.Should().HaveCount(2);
        delays.Should().OnlyContain(d => d >= TimeSpan.FromMilliseconds(75) && d <= TimeSpan.FromMilliseconds(250));
    }

    //==============================================================================================
    /// <summary>
    /// CustomDelayRetryPolicy WithMaxDelay should cap delays correctly.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void CustomDelayRetryPolicy_WithMaxDelay_should_cap_delays()
    {
        var customDelays = new[] { TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(800) };
        var policy = RetryPolicy.WaitAndRetry(customDelays);
        var delays = new List<TimeSpan>();

        var newPolicy = policy.WithMaxDelay(TimeSpan.FromMilliseconds(300)).OnRetry((ex, attempt, delay) => delays.Add(delay));

        newPolicy.Should().NotBeSameAs(policy);

        try
        {
            newPolicy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        delays.Should().HaveCount(3);
        delays[0].Should().Be(TimeSpan.FromMilliseconds(100));
        delays[1].Should().Be(TimeSpan.FromMilliseconds(300));
        delays[2].Should().Be(TimeSpan.FromMilliseconds(300));
    }

    //==============================================================================================
    /// <summary>
    /// CustomDelayRetryPolicy GetDelayForAttempt should return correct delay for attempt.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void CustomDelayRetryPolicy_GetDelayForAttempt_should_return_correct_delay()
    {
        var customDelays = new[] { TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(300) };
        var policy = RetryPolicy.WaitAndRetry(customDelays);

        var delays = new List<TimeSpan>();
        var newPolicy = policy.OnRetry((ex, attempt, delay) => delays.Add(delay));

        try
        {
            newPolicy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        delays.Should().HaveCount(3);
        delays[0].Should().Be(TimeSpan.FromMilliseconds(100));
        delays[1].Should().Be(TimeSpan.FromMilliseconds(200));
        delays[2].Should().Be(TimeSpan.FromMilliseconds(300));
    }

    //==============================================================================================
    /// <summary>
    /// CustomDelayRetryPolicy WithMaxDelay should throw for zero or negative delay.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void CustomDelayRetryPolicy_WithMaxDelay_should_throw_for_invalid_delay()
    {
        var customDelays = new[] { TimeSpan.FromMilliseconds(100) };
        var policy = RetryPolicy.WaitAndRetry(customDelays);

        var act = () => policy.WithMaxDelay(TimeSpan.Zero);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("maxDelay");
    }

    //==============================================================================================
    /// <summary>
    /// RetryIfPolicy WithMaxDelay should throw for zero or negative delay.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryIfPolicy_WithMaxDelay_should_throw_for_invalid_delay()
    {
        var policy = RetryPolicy.Retry(2).RetryIf<int>(x => x < 0);

        var act = () => policy.WithMaxDelay(TimeSpan.FromMilliseconds(-1));

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("maxDelay");
    }

    //==============================================================================================
    /// <summary>
    /// RetryPolicy Retry should throw for negative retry count.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryPolicy_Retry_should_throw_for_negative_retry_count()
    {
        var act = () => RetryPolicy.Retry(-1);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("count");
    }

    //==============================================================================================
    /// <summary>
    /// RetryPolicy Retry should throw for negative delay.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryPolicy_Retry_should_throw_for_negative_delay()
    {
        var act = () => RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(-100));

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("delay");
    }

    //==============================================================================================
    /// <summary>
    /// RetryPolicy Retry with delay should throw for negative retry count.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryPolicy_Retry_with_delay_should_throw_for_negative_count()
    {
        var act = () => RetryPolicy.Retry(-1, TimeSpan.FromMilliseconds(100));

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("count");
    }

    //==============================================================================================
    /// <summary>
    /// CustomDelayRetryPolicy with timeout and callback should use second constructor.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void CustomDelayRetryPolicy_with_timeout_and_callback_should_work()
    {
        var customDelays = new[] { TimeSpan.FromMilliseconds(100) };
        var policy = RetryPolicy.WaitAndRetry(customDelays)
            .Handle<InvalidOperationException>()
            .OnRetry((ex, attempt, delay) => { });

        var callbackInvoked = false;
        var newPolicy = policy.OnRetry((ex, attempt, delay) => callbackInvoked = true);

        try
        {
            newPolicy.Execute(() => throw new InvalidOperationException("Test"));
        }
        catch
        {
        }

        callbackInvoked.Should().BeTrue();
    }

    //==============================================================================================
    /// <summary>
    /// CustomDelayRetryPolicy GetDelayForAttempt should return zero for out of range attempt.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void CustomDelayRetryPolicy_GetDelayForAttempt_should_return_zero_for_out_of_range()
    {
        var customDelays = new[] { TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200) };
        var policy = (CustomDelayRetryPolicy)RetryPolicy.WaitAndRetry(customDelays);

        policy.GetDelayForAttempt(0).Should().Be(TimeSpan.Zero);
        policy.GetDelayForAttempt(3).Should().Be(TimeSpan.Zero);
        policy.GetDelayForAttempt(-1).Should().Be(TimeSpan.Zero);
    }

    //==============================================================================================
    /// <summary>
    /// RetryPolicyBase ExecuteWithResult should throw for null action.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryPolicyBase_ExecuteWithResult_should_throw_for_null_action()
    {
        var policy = RetryPolicy.Retry(3);

        var act = () => policy.ExecuteWithResult((Action)null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("action");
    }

    //==============================================================================================
    /// <summary>
    /// RetryExecutor should throw when no exception is captured.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryExecutor_should_throw_when_no_exception_captured()
    {
        var policy = RetryPolicy.Retry(0);

        var act = () => policy.Execute(() => throw new InvalidOperationException("Test"));

        act.Should().Throw<InvalidOperationException>().WithMessage("Test");
    }

    //==============================================================================================
    /// <summary>
    /// RetryExecutor ExecuteWithResult should return failure when all retries exhausted.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryExecutor_ExecuteWithResult_should_return_failure_when_retries_exhausted()
    {
        var policy = RetryPolicy.Retry(2);

        var result = policy.ExecuteWithResult(() => throw new InvalidOperationException("Test"));

        result.Success.Should().BeFalse();
        result.Attempts.Should().Be(3);
        result.LastException.Should().BeOfType<InvalidOperationException>();
    }

    //==============================================================================================
    /// <summary>
    /// RetryIfPolicy ExecuteAsync should throw when no exception captured.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task RetryIfPolicy_ExecuteAsync_should_throw_when_no_exception_captured()
    {
        var policy = RetryPolicy.Retry(0).RetryIf<int>(x => x < 0);

        var act = async () => await policy.ExecuteAsync(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Test");
        });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Test");
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResult with Func should return failure when all retries exhausted.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExecuteWithResult_Func_should_return_failure_when_exhausted()
    {
        var policy = RetryPolicy.Retry(1, TimeSpan.FromMilliseconds(10));
        var attempts = 0;

        var result = policy.ExecuteWithResult<string>(() =>
        {
            attempts++;
            throw new InvalidOperationException($"Attempt {attempts}");
        });

        result.Success.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Attempts.Should().Be(2);
        result.LastException.Should().BeOfType<InvalidOperationException>();
    }

    //==============================================================================================
    /// <summary>
    /// Execute without timeout should use null cancellation token source path.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Execute_without_timeout_should_succeed()
    {
        var policy = RetryPolicy.Retry(2);
        var executed = false;

        var result = policy.Execute(() =>
        {
            executed = true;
            return "success";
        });

        executed.Should().BeTrue();
        result.Should().Be("success");
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResult without timeout should use null cancellation token source path.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExecuteWithResult_without_timeout_should_track_attempts()
    {
        var policy = RetryPolicy.Retry(3);
        var attempts = 0;

        var result = policy.ExecuteWithResult(() =>
        {
            attempts++;

            if (attempts < 2)
            {
                throw new InvalidOperationException("Fail");
            }

            return "success";
        });

        result.Success.Should().BeTrue();
        result.Value.Should().Be("success");
        result.Attempts.Should().Be(2);
        result.LastException.Should().BeNull();
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteAsync with delay should wait between retries.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteAsync_with_delay_should_wait_between_retries()
    {
        var policy = RetryPolicy.Retry(2, TimeSpan.FromMilliseconds(10));
        var attempts = 0;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var result = await policy.ExecuteAsync(async () =>
        {
            attempts++;
            await Task.Delay(1);

            if (attempts < 3)
            {
                throw new InvalidOperationException("Retry");
            }

            return "success";
        });

        stopwatch.Stop();
        result.Should().Be("success");
        attempts.Should().Be(3);
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(20);
    }

    //==============================================================================================
    /// <summary>
    /// RetryIfPolicy ExecuteAsync with result predicate should retry based on value.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task RetryIfPolicy_ExecuteAsync_should_retry_based_on_result_value()
    {
        var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(10))
            .RetryIf<int>(x => x < 10);

        var attempts = 0;

        var result = await policy.ExecuteAsync(async () =>
        {
            attempts++;
            await Task.Delay(1);
            return attempts * 5;
        });

        attempts.Should().Be(2);
        result.Should().Be(10);
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResult with timeout should succeed for fast operation.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteWithResult_with_timeout_should_succeed_for_fast_operation()
    {
        var policy = RetryPolicy.Retry(2).WithTimeout(TimeSpan.FromSeconds(5));

        var result = await policy.ExecuteWithResultAsync(async () =>
        {
            await Task.Delay(10);
            return "success";
        });

        result.Success.Should().BeTrue();
        result.Value.Should().Be("success");
    }

    //==============================================================================================
    /// <summary>
    /// Execute with cancellation token should check token on retry.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Execute_with_cancellation_token_should_check_token()
    {
        using var cts = new CancellationTokenSource();
        var policy = RetryPolicy.Retry(5, TimeSpan.FromMilliseconds(10));
        var attempts = 0;

        var act = () => policy.Execute(() =>
        {
            attempts++;

            if (attempts == 2)
            {
                cts.Cancel();
            }

            throw new InvalidOperationException("Test");
        }, cts.Token);

        act.Should().Throw<OperationCanceledException>();
        attempts.Should().Be(2);
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResult should handle unhandled exception types immediately.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExecuteWithResult_should_handle_unhandled_exception()
    {
        var policy = RetryPolicy.Retry(2).Handle<InvalidOperationException>();

        var result = policy.ExecuteWithResult<string>(() => throw new ArgumentException("Different exception"));

        result.Success.Should().BeFalse();
        result.Attempts.Should().Be(1);
        result.LastException.Should().BeOfType<ArgumentException>();
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResultAsync with timeout should succeed for fast operation.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteWithResultAsync_with_timeout_should_succeed()
    {
        var policy = RetryPolicy.Retry(3).WithTimeout(TimeSpan.FromSeconds(5));

        var result = await policy.ExecuteWithResultAsync(async () =>
        {
            await Task.Delay(10);
            return "success";
        });

        result.Success.Should().BeTrue();
        result.Value.Should().Be("success");
        result.Attempts.Should().Be(1);
    }

    //==============================================================================================
    /// <summary>
    /// RetryIfPolicy ExecuteAsync with delay should retry with delays.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task RetryIfPolicy_ExecuteAsync_with_delay_should_retry()
    {
        var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(10))
            .RetryIf<string>(s => s == "retry");

        var attempts = 0;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var result = await policy.ExecuteAsync(async () =>
        {
            attempts++;
            await Task.Delay(1);
            return attempts < 3 ? "retry" : "success";
        });

        stopwatch.Stop();
        result.Should().Be("success");
        attempts.Should().Be(3);
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(20);
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResult with OnRetry callback should invoke callback on retry.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExecuteWithResult_with_OnRetry_should_invoke_callback()
    {
        var policy = RetryPolicy.Retry(2, TimeSpan.FromMilliseconds(10));
        var callbackInvoked = false;
        var policyWithCallback = policy.OnRetry((ex, attempt, delay) => callbackInvoked = true);

        var result = policyWithCallback.ExecuteWithResult<string>(() => throw new InvalidOperationException("Test"));

        result.Success.Should().BeFalse();
        callbackInvoked.Should().BeTrue();
    }

    //==============================================================================================
    /// <summary>
    /// Execute with zero delay should skip Thread.Sleep.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Execute_with_zero_delay_should_skip_sleep()
    {
        var policy = RetryPolicy.Retry(2, TimeSpan.Zero);
        var attempts = 0;

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
    /// RetryIfPolicy Execute with zero delay should skip Thread.Sleep.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryIfPolicy_Execute_with_zero_delay_should_skip_sleep()
    {
        var policy = RetryPolicy.Retry(2, TimeSpan.Zero).RetryIf<int>(x => x < 0);
        var attempts = 0;

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
    /// ExecuteAsync with timeout should cancel when operation exceeds timeout.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteAsync_with_timeout_should_cancel_operation()
    {
        var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(10)).WithTimeout(TimeSpan.FromMilliseconds(50));
        var attempts = 0;

        var act = async () => await policy.ExecuteAsync(async () =>
        {
            attempts++;
            await Task.Delay(200);
            return "success";
        });

        await act.Should().ThrowAsync<OperationCanceledException>();
        attempts.Should().BeGreaterThanOrEqualTo(1);
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResultAsync with timeout should cancel when operation exceeds timeout.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteWithResultAsync_with_timeout_should_cancel_operation()
    {
        var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(10)).WithTimeout(TimeSpan.FromMilliseconds(50));
        var attempts = 0;

        var act = async () => await policy.ExecuteWithResultAsync(async () =>
        {
            attempts++;
            await Task.Delay(200);
            return "success";
        });

        await act.Should().ThrowAsync<TaskCanceledException>();
        attempts.Should().BeGreaterThanOrEqualTo(1);
    }

    //==============================================================================================
    /// <summary>
    /// Execute should exit loop early when operation succeeds before retry count exhausted.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Execute_should_exit_early_on_success()
    {
        var policy = RetryPolicy.Retry(5, TimeSpan.FromMilliseconds(10));
        var attempts = 0;

        var result = policy.Execute(() =>
        {
            attempts++;

            if (attempts < 3)
            {
                throw new InvalidOperationException("Retry");
            }

            return "success";
        });

        result.Should().Be("success");
        attempts.Should().Be(3);
    }

    //==============================================================================================
    /// <summary>
    /// ExecuteWithResult should exit loop early when operation succeeds.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExecuteWithResult_should_exit_early_on_success()
    {
        var policy = RetryPolicy.Retry(5, TimeSpan.FromMilliseconds(10));
        var attempts = 0;

        var result = policy.ExecuteWithResult(() =>
        {
            attempts++;

            if (attempts < 3)
            {
                throw new InvalidOperationException("Retry");
            }

            return "success";
        });

        result.Success.Should().BeTrue();
        result.Value.Should().Be("success");
        result.Attempts.Should().Be(3);
    }

    //==============================================================================================
    /// <summary>
    /// RetryIfPolicy ExecuteAsync with timeout should cancel operation.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task RetryIfPolicy_ExecuteAsync_with_timeout_should_cancel()
    {
        var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(10)).WithTimeout(TimeSpan.FromMilliseconds(50)).RetryIf<string>(x => x == "retry");
        var attempts = 0;

        var act = async () => await policy.ExecuteAsync(async () =>
        {
            attempts++;
            await Task.Delay(200);
            return "success";
        });

        await act.Should().ThrowAsync<OperationCanceledException>();
        attempts.Should().BeGreaterThanOrEqualTo(1);
    }

    //==============================================================================================
    /// <summary>
    /// RetryIfPolicy Execute with callback should invoke callback on early success.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryIfPolicy_Execute_with_callback_should_invoke_on_retry()
    {
        var callbackCount = 0;
        var policy = RetryPolicy.Retry(5, TimeSpan.FromMilliseconds(10))
            .RetryIf<string>(x => x == "retry")
            .OnRetry((ex, attempt, delay) => callbackCount++);
        var attempts = 0;

        var result = policy.Execute(() =>
        {
            attempts++;

            if (attempts < 3)
            {
                return "retry";
            }

            return "success";
        });

        result.Should().Be("success");
        attempts.Should().Be(3);
        callbackCount.Should().Be(2);
    }
}
