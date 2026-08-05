using FluentAssertions;
using Pinkterest.Application.Common.Results;
using Xunit;

namespace Pinkterest.UnitTests.Common;

public class ResultTests
{
    [Fact]
    public void Success_carries_the_value()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_refuses_to_expose_a_value()
    {
        var result = Result.Failure<int>(Error.NotFound("Photo"));

        result.IsFailure.Should().BeTrue();

        var readValue = () => result.Value;
        readValue.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Bind_short_circuits_on_failure()
    {
        var error = Error.Validation("Quota.Exceeded", "Daily upload limit reached.");

        var result = Result.Failure<int>(error).Bind(value => Result.Success(value * 2));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Map_projects_a_successful_value()
    {
        Result.Success(21).Map(value => value * 2).Value.Should().Be(42);
    }

    [Fact]
    public void Ensure_turns_a_broken_invariant_into_a_failure()
    {
        var error = Error.Validation("Value.TooSmall", "Must be positive.");

        var result = Result.Success(-1).Ensure(value => value > 0, error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Match_selects_the_matching_branch()
    {
        var outcome = Result.Failure<int>(Error.Forbidden("upload")).Match(
            onSuccess: value => $"ok:{value}",
            onFailure: error => $"error:{error.Code}");

        outcome.Should().Be("error:upload.Forbidden");
    }
}
