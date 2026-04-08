using Blinder.SharedKernel;
using Xunit;

namespace Blinder.Api.Tests;

/// <summary>
/// Tests for SharedKernel foundational primitives.
/// SharedKernel is referenced by Blinder.Api; these tests verify the primitives work correctly.
/// </summary>
public class ResultTests
{
    [Fact]
    public void Result_Success_IsSuccessful()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Result_Failure_HasError()
    {
        var error = new Error("TEST_001", "Something went wrong");
        var result = Result<int>.Failure(error);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("TEST_001", result.Error.Code);
        Assert.Equal("Something went wrong", result.Error.Message);
    }

    [Fact]
    public void Result_Success_AccessingError_Throws()
    {
        var result = Result<int>.Success(1);
        Assert.Throws<InvalidOperationException>(() => result.Error);
    }

    [Fact]
    public void Result_Failure_AccessingValue_Throws()
    {
        var error = new Error("ERR", "fail");
        var result = Result<int>.Failure(error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Result_ImplicitConversionFromValue_IsSuccess()
    {
        Result<string> result = "hello";
        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void Result_ImplicitConversionFromError_IsFailure()
    {
        Result<string> result = new Error("X", "bad");
        Assert.True(result.IsFailure);
    }
}

public class ErrorTests
{
    [Fact]
    public void Error_ToString_IncludesCodeAndMessage()
    {
        var error = new Error("AUTH_001", "Unauthorized");
        Assert.Equal("AUTH_001: Unauthorized", error.ToString());
    }

    [Fact]
    public void Error_None_HasEmptyCodeAndMessage()
    {
        Assert.Equal(string.Empty, Error.None.Code);
        Assert.Equal(string.Empty, Error.None.Message);
    }
}

public class CorrelationIdTests
{
    [Fact]
    public void CorrelationId_New_GeneratesUniqueIds()
    {
        var id1 = CorrelationId.New();
        var id2 = CorrelationId.New();
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void CorrelationId_Parse_RoundTrips()
    {
        var id = CorrelationId.New();
        var parsed = CorrelationId.Parse(id.ToString());
        Assert.Equal(id, parsed);
    }

    [Fact]
    public void CorrelationId_TryParse_ValidGuid_ReturnsTrueAndValue()
    {
        var guid = Guid.NewGuid();
        var success = CorrelationId.TryParse(guid.ToString(), out var result);
        Assert.True(success);
        Assert.Equal(guid, result.Value);
    }

    [Fact]
    public void CorrelationId_TryParse_InvalidInput_ReturnsFalse()
    {
        var success = CorrelationId.TryParse("not-a-guid", out _);
        Assert.False(success);
    }

    [Fact]
    public void CorrelationId_EqualityOperators_Work()
    {
        var guid = Guid.NewGuid();
        var id1 = new CorrelationId(guid);
        var id2 = new CorrelationId(guid);
        Assert.True(id1 == id2);
        Assert.False(id1 != id2);
    }
}
