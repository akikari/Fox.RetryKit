//==================================================================================================
// Tests for CancellationToken support in retry operations.
// Verifies that cancellation is properly propagated and honored.
//==================================================================================================

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for CancellationToken support.
/// </summary>
//==================================================================================================
public sealed class CancellationTests
{
    //==============================================================================================
    /// <summary>
    /// Tests that cancelled token throws OperationCanceledException immediately.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Execute_with_cancelled_token_should_throw_immediately()
    {
        var policy = RetryPolicy.Retry(3);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var attempts = 0;

        var act = () => policy.Execute(() =>
        {
            attempts++;
            throw new InvalidOperationException("Test");
        }, cts.Token);

        act.Should().Throw<OperationCanceledException>();
        attempts.Should().Be(0);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that cancellation during retry loop stops execution.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteAsync_should_honor_cancellation_during_retry()
    {
        var policy = RetryPolicy.Retry(10, TimeSpan.FromMilliseconds(100));
        var cts = new CancellationTokenSource();
        var attempts = 0;

        var task = policy.ExecuteAsync(async () =>
        {
            attempts++;

            if (attempts == 2)
            {
                cts.Cancel();
            }

            await Task.Delay(1);
            throw new InvalidOperationException("Test");
        }, cts.Token);

        var act = async () => await task;
        await act.Should().ThrowAsync<OperationCanceledException>();

        attempts.Should().BeInRange(2, 3);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that successful operation completes despite available cancellation token.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task ExecuteAsync_should_complete_if_operation_succeeds_before_cancellation()
    {
        var policy = RetryPolicy.Retry(3);
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var result = await policy.ExecuteAsync(async () =>
        {
            await Task.Delay(10);
            return 42;
        }, cts.Token);

        result.Should().Be(42);
    }
}
