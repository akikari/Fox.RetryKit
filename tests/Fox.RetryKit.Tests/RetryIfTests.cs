//==================================================================================================
// Unit tests for RetryIf conditional retry feature.
// Verifies retry logic based on result predicates.
//==================================================================================================

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for RetryIf conditional retry functionality.
/// </summary>
//==================================================================================================
public sealed class RetryIfTests
{
    //==============================================================================================
    /// <summary>
    /// RetryIf should retry when predicate returns true.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryIf_should_retry_when_predicate_is_true()
    {
        var attempts = 0;
        var policy = RetryPolicy.Retry(3).RetryIf<int>(result => result < 5);

        var result = policy.Execute(() =>
        {
            attempts++;
            return attempts;
        });

        attempts.Should().Be(4);
        result.Should().Be(4);
    }

    //==============================================================================================
    /// <summary>
    /// RetryIf should not retry when predicate returns false.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryIf_should_not_retry_when_predicate_is_false()
    {
        var attempts = 0;
        var policy = RetryPolicy.Retry(5).RetryIf<int>(result => result < 0);

        var result = policy.Execute(() =>
        {
            attempts++;
            return attempts;
        });

        attempts.Should().Be(1);
        result.Should().Be(1);
    }

    //==============================================================================================
    /// <summary>
    /// RetryIf should respect retry count limit.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryIf_should_respect_retry_count_limit()
    {
        var attempts = 0;
        var policy = RetryPolicy.Retry(3).RetryIf<int>(result => result < 100);

        var result = policy.Execute(() =>
        {
            attempts++;
            return attempts;
        });

        attempts.Should().Be(4);
        result.Should().Be(4);
    }

    //==============================================================================================
    /// <summary>
    /// RetryIf should work with delays between retries.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryIf_should_work_with_delays()
    {
        var attempts = 0;
        var policy = RetryPolicy.Retry(3, TimeSpan.FromMilliseconds(50)).RetryIf<int>(result => result < 3);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var result = policy.Execute(() =>
        {
            attempts++;
            return attempts;
        });

        sw.Stop();
        attempts.Should().Be(3);
        result.Should().Be(3);
        sw.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(100);
    }

    //==============================================================================================
    /// <summary>
    /// RetryIf should throw ArgumentNullException when predicate is null.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryIf_should_throw_when_predicate_is_null()
    {
        var policy = RetryPolicy.Retry(3);

        var act = () => policy.RetryIf<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    //==============================================================================================
    /// <summary>
    /// RetryIf should work asynchronously.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task RetryIf_should_work_with_async_execution()
    {
        var attempts = 0;
        var policy = RetryPolicy.Retry(3).RetryIf<int>(result => result < 5);

        var result = await policy.ExecuteAsync(async () =>
        {
            attempts++;
            await Task.Yield();
            return attempts;
        });

        attempts.Should().Be(4);
        result.Should().Be(4);
    }

    //==============================================================================================
    /// <summary>
    /// RetryIf should still retry on exceptions.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryIf_should_still_retry_on_exceptions()
    {
        var attempts = 0;
        var policy = RetryPolicy.Retry(3).RetryIf<int>(result => result < 0);

        try
        {
            policy.Execute(() =>
            {
                attempts++;

                if (attempts < 3)
                {
                    throw new InvalidOperationException("Fail");
                }

                return 100;
            });
        }
        catch
        {
        }

        attempts.Should().Be(3);
    }

    //==============================================================================================
    /// <summary>
    /// RetryIf should combine with exception filtering.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void RetryIf_should_combine_with_exception_filtering()
    {
        var attempts = 0;
        var policy = RetryPolicy.Retry(5).Handle<InvalidOperationException>().RetryIf<int>(result => result < 5);

        try
        {
            policy.Execute(() =>
            {
                attempts++;

                if (attempts < 3)
                {
                    throw new InvalidOperationException("Fail");
                }

                if (attempts == 3)
                {
                    throw new ArgumentException("Unhandled");
                }

                return attempts;
            });
        }
        catch (ArgumentException)
        {
        }

        attempts.Should().Be(3);
    }
}
