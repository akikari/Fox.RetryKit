//==================================================================================================
// Tests for exception filtering functionality.
// Verifies that only specified exception types trigger retries.
//==================================================================================================

namespace Fox.RetryKit.Tests;

//==================================================================================================
/// <summary>
/// Tests for exception filtering functionality.
/// </summary>
//==================================================================================================
public sealed class ExceptionFilterTests
{
    //==============================================================================================
    /// <summary>
    /// Tests that Handle filters specific exception type.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Handle_should_retry_only_specified_exception()
    {
        var policy = RetryPolicy.Retry(3).Handle<InvalidOperationException>();
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
    /// Tests that unhandled exceptions are not retried.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Handle_should_not_retry_unhandled_exception()
    {
        var policy = RetryPolicy.Retry(3).Handle<InvalidOperationException>();
        var attempts = 0;

        var act = () => policy.Execute(() =>
        {
            attempts++;
            throw new ArgumentException("Test exception");
        });

        act.Should().Throw<ArgumentException>();
        attempts.Should().Be(1);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that Handle can filter multiple exception types.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Handle_should_support_multiple_exception_types()
    {
        var policy = RetryPolicy.Retry(3)
            .Handle<InvalidOperationException>()
            .Handle<ArgumentException>();

        var attempts = 0;

        var act1 = () => policy.Execute(() =>
        {
            attempts++;
            throw new InvalidOperationException("Test");
        });

        act1.Should().Throw<InvalidOperationException>();
        attempts.Should().Be(4);

        attempts = 0;

        var act2 = () => policy.Execute(() =>
        {
            attempts++;
            throw new ArgumentException("Test");
        });

        act2.Should().Throw<ArgumentException>();
        attempts.Should().Be(4);
    }

    //==============================================================================================
    /// <summary>
    /// Tests that Handle works with derived exception types.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Handle_should_catch_derived_exceptions()
    {
        var policy = RetryPolicy.Retry(3).Handle<Exception>();
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
    /// Tests that policy without Handle retries all exceptions.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void Policy_without_handle_should_retry_all_exceptions()
    {
        var policy = RetryPolicy.Retry(3);
        var attempts = 0;

        var act = () => policy.Execute(() =>
        {
            attempts++;
            throw new InvalidOperationException("Test");
        });

        act.Should().Throw<InvalidOperationException>();
        attempts.Should().Be(4);
    }
}
