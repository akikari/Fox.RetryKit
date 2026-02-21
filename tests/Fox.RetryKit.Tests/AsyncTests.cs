//==================================================================================================
// Tests for asynchronous retry operations.
// Verifies async/await behavior with retries and delays.
//==================================================================================================

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for asynchronous retry operations.
/// </summary>
//==================================================================================================
public sealed class AsyncTests
{
    //==============================================================================================
    /// <summary>
    /// Tests that ExecuteAsync retries on failure.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteAsync_should_retry_on_failure()
    {
        var policy = RetryPolicy.Retry(3);
        var attempts = 0;

        var act = async () => await policy.ExecuteAsync(async () =>
        {
            attempts++;
            await Task.Delay(1);
            throw new InvalidOperationException("Test");
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
        attempts.Should().Be(4);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that ExecuteAsync returns result on success.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteAsync_should_return_result_on_success()
    {
        var policy = RetryPolicy.Retry(3);

        var result = await policy.ExecuteAsync(async () =>
        {
            await Task.Delay(1);
            return 42;
        });

        result.Should().Be(42);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that ExecuteAsync succeeds after retries.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteAsync_should_succeed_after_retries()
    {
        var policy = RetryPolicy.Retry(3);
        var attempts = 0;

        var result = await policy.ExecuteAsync(async () =>
        {
            attempts++;
            await Task.Delay(1);

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
    /// Tests that null async function throws ArgumentNullException.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteAsync_with_null_func_should_throw()
    {
        var policy = RetryPolicy.Retry(3);
        var act = async () => await policy.ExecuteAsync((Func<Task<int>>)null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    //==============================================================================================
    /// <summary>
    /// Tests that null async action throws ArgumentNullException.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteAsync_with_null_action_should_throw()
    {
        var policy = RetryPolicy.Retry(3);
        var act = async () => await policy.ExecuteAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
