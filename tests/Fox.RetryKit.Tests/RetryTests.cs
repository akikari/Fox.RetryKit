//==================================================================================================
// Tests for basic retry functionality without delay.
// Verifies retry count behavior and exception handling.
//==================================================================================================

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for basic retry functionality.
/// </summary>
//==================================================================================================
public sealed class RetryTests
{
    //==============================================================================================
    /// <summary>
    /// Tests that Retry with count 0 throws immediately on failure.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Retry_with_zero_count_should_throw_immediately()
    {
        var policy = RetryPolicy.Retry(0);
        var attempts = 0;

        var act = () => policy.Execute(() =>
        {
            attempts++;
            throw new InvalidOperationException("Test exception");
        });

        act.Should().Throw<InvalidOperationException>();
        attempts.Should().Be(1);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that Retry with count 3 attempts 4 times total (initial + 3 retries).
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Retry_with_count_3_should_attempt_4_times()
    {
        var policy = RetryPolicy.Retry(3);
        var attempts = 0;

        var act = () => policy.Execute(() =>
        {
            attempts++;
            throw new InvalidOperationException("Test exception");
        });

        act.Should().Throw<InvalidOperationException>();
        attempts.Should().Be(4);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that successful operation on first attempt does not retry.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Retry_should_not_retry_on_success()
    {
        var policy = RetryPolicy.Retry(3);
        var attempts = 0;

        var result = policy.Execute(() =>
        {
            attempts++;
            return 42;
        });

        result.Should().Be(42);
        attempts.Should().Be(1);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that operation succeeds after 2 failures.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Retry_should_succeed_after_failures()
    {
        var policy = RetryPolicy.Retry(3);
        var attempts = 0;

        var result = policy.Execute(() =>
        {
            attempts++;

            if (attempts < 3)
            {
                throw new InvalidOperationException("Temporary failure");
            }

            return 42;
        });

        result.Should().Be(42);
        attempts.Should().Be(3);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that negative retry count throws ArgumentOutOfRangeException.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Retry_with_negative_count_should_throw()
    {
        var act = () => RetryPolicy.Retry(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    //==============================================================================================
    /// <summary>
    /// Tests that null action throws ArgumentNullException.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Execute_with_null_action_should_throw()
    {
        var policy = RetryPolicy.Retry(3);
        var act = () => policy.Execute(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    //==============================================================================================
    /// <summary>
    /// Tests that null function throws ArgumentNullException.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Execute_with_null_func_should_throw()
    {
        var policy = RetryPolicy.Retry(3);
        var act = () => policy.Execute((Func<int>)null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
