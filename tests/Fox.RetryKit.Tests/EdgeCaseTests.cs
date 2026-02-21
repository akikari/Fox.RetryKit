//==================================================================================================
// Tests for edge cases and boundary conditions.
// Verifies behavior in unusual or extreme scenarios.
//==================================================================================================

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for edge cases and boundary conditions.
/// </summary>
//==================================================================================================
public sealed class EdgeCaseTests
{
    //==============================================================================================
    /// <summary>
    /// Tests that zero delay is handled correctly.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Retry_with_zero_delay_should_work()
    {
        var policy = RetryPolicy.Retry(3, TimeSpan.Zero);
        var attempts = 0;

        var act = () => policy.Execute(() =>
        {
            attempts++;
            throw new InvalidOperationException("Test");
        });

        act.Should().Throw<InvalidOperationException>();
        attempts.Should().Be(4);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that zero initial delay for exponential backoff works.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void ExponentialBackoff_with_zero_delay_should_work()
    {
        var policy = RetryPolicy.ExponentialBackoff(3, TimeSpan.Zero);
        var attempts = 0;

        var act = () => policy.Execute(() =>
        {
            attempts++;
            throw new InvalidOperationException("Test");
        });

        act.Should().Throw<InvalidOperationException>();
        attempts.Should().Be(4);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that very large retry count is handled.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Retry_with_large_count_should_work()
    {
        var policy = RetryPolicy.Retry(1000);
        var attempts = 0;

        var result = policy.Execute(() =>
        {
            attempts++;

            if (attempts < 5)
            {
                throw new InvalidOperationException("Test");
            }

            return 42;
        });

        result.Should().Be(42);
        attempts.Should().Be(5);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that policy can be reused for multiple executions.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Policy_should_be_reusable()
    {
        var policy = RetryPolicy.Retry(2);

        var result1 = policy.Execute(() => 1);
        var result2 = policy.Execute(() => 2);

        result1.Should().Be(1);
        result2.Should().Be(2);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that Handle can be chained multiple times.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Handle_should_support_chaining()
    {
        var policy = RetryPolicy.Retry(3)
            .Handle<InvalidOperationException>()
            .Handle<ArgumentException>()
            .Handle<TimeoutException>();

        policy.HandledExceptions.Should().HaveCount(3);
        policy.HandledExceptions.Should().Contain(typeof(InvalidOperationException));
        policy.HandledExceptions.Should().Contain(typeof(ArgumentException));
        policy.HandledExceptions.Should().Contain(typeof(TimeoutException));
    }

    //==============================================================================================
    /// <summary>
    /// Tests that combining retry with timeout works correctly.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task Retry_with_timeout_should_cancel_on_timeout()
    {
        var policy = RetryPolicy.Retry(10, TimeSpan.FromMilliseconds(50))
            .WithTimeout(TimeSpan.FromMilliseconds(200));

        var attempts = 0;

        var act = async () => await policy.ExecuteAsync(async () =>
        {
            attempts++;
            await Task.Delay(1);
            throw new InvalidOperationException("Test");
        });

        await act.Should().ThrowAsync<OperationCanceledException>();
        attempts.Should().BeLessThan(10);
    }
}
