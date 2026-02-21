//==================================================================================================
// Unit tests for Fallback feature.
// Verifies fallback value return on retry exhaustion.
//==================================================================================================

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for Fallback functionality.
/// </summary>
//==================================================================================================
public sealed class FallbackTests
{
    //==============================================================================================
    /// <summary>
    /// Fallback should return fallback value when all retries fail.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Fallback_should_return_fallback_value_on_failure()
    {
        var policy = RetryPolicy.Retry(2);

        var result = policy.Fallback(() => throw new InvalidOperationException("Always fails"), "fallback");

        result.Should().Be("fallback");
    }

    //==============================================================================================
    /// <summary>
    /// Fallback should return actual value on success.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Fallback_should_return_actual_value_on_success()
    {
        var policy = RetryPolicy.Retry(2);

        var result = policy.Fallback(() => "success", "fallback");

        result.Should().Be("success");
    }

    //==============================================================================================
    /// <summary>
    /// Fallback should retry before using fallback value.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Fallback_should_retry_before_returning_fallback()
    {
        var attempts = 0;
        var policy = RetryPolicy.Retry(3);

        var result = policy.Fallback(() =>
        {
            attempts++;
            throw new InvalidOperationException("Fail");
        }, "fallback");

        attempts.Should().Be(4);
        result.Should().Be("fallback");
    }

    //==============================================================================================
    /// <summary>
    /// Fallback with provider should invoke provider function on failure.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Fallback_with_provider_should_invoke_provider_on_failure()
    {
        var policy = RetryPolicy.Retry(1);
        var providerInvoked = false;

        var result = policy.Fallback(() => throw new InvalidOperationException("Fail"), () =>
        {
            providerInvoked = true;
            return "fallback";
        });

        providerInvoked.Should().BeTrue();
        result.Should().Be("fallback");
    }

    //==============================================================================================
    /// <summary>
    /// Fallback should throw ArgumentNullException when func is null.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Fallback_should_throw_when_func_is_null()
    {
        var policy = RetryPolicy.Retry(2);

        var act = () => policy.Fallback<string>(null!, "fallback");

        act.Should().Throw<ArgumentNullException>();
    }

    //==============================================================================================
    /// <summary>
    /// Fallback should throw ArgumentNullException when provider is null.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Fallback_should_throw_when_provider_is_null()
    {
        var policy = RetryPolicy.Retry(2);

        var act = () => policy.Fallback(() => "test", (Func<string>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    //==============================================================================================
    /// <summary>
    /// FallbackAsync should return fallback value when all retries fail.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task FallbackAsync_should_return_fallback_value_on_failure()
    {
        var policy = RetryPolicy.Retry(2);

        var result = await policy.FallbackAsync(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("Always fails");
        }, "fallback");

        result.Should().Be("fallback");
    }

    //==============================================================================================
    /// <summary>
    /// FallbackAsync should return actual value on success.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task FallbackAsync_should_return_actual_value_on_success()
    {
        var policy = RetryPolicy.Retry(2);

        var result = await policy.FallbackAsync(async () =>
        {
            await Task.Yield();
            return "success";
        }, "fallback");

        result.Should().Be("success");
    }

    //==============================================================================================
    /// <summary>
    /// FallbackAsync with provider should invoke async provider on failure.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task FallbackAsync_with_provider_should_invoke_async_provider()
    {
        var policy = RetryPolicy.Retry(1);
        var providerInvoked = false;

        var result = await policy.FallbackAsync(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("Fail");
        }, async () =>
        {
            await Task.Yield();
            providerInvoked = true;
            return "fallback";
        });

        providerInvoked.Should().BeTrue();
        result.Should().Be("fallback");
    }

    //==============================================================================================
    /// <summary>
    /// FallbackAsync should work with retry delays.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task FallbackAsync_should_work_with_retry_delays()
    {
        var attempts = 0;
        var policy = RetryPolicy.Retry(2, TimeSpan.FromMilliseconds(50));
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var result = await policy.FallbackAsync(async () =>
        {
            attempts++;
            await Task.Yield();
            throw new InvalidOperationException("Fail");
        }, "fallback");

        sw.Stop();
        attempts.Should().Be(3);
        result.Should().Be("fallback");
        sw.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(100);
    }
}
