using Backend.Application.Common;

namespace Backend.Tests.Unit.Application.Common;

public class Result_Tests
{
    [Fact]
    public void Ok_Should_Return_Success_Result()
    {
        var result = Result.Ok();

        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void BadRequest_Should_Return_Failure_With_BadRequest_ErrorType()
    {
        var result = Result.BadRequest("Invalid input.");

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Equal("Invalid input.", result.ErrorMessage);
    }

    [Fact]
    public void NotFound_Should_Return_Failure_With_NotFound_ErrorType()
    {
        var result = Result.NotFound("Resource not found.");

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Equal("Resource not found.", result.ErrorMessage);
    }

    [Fact]
    public void Conflict_Should_Return_Failure_With_Conflict_ErrorType()
    {
        var result = Result.Conflict("Resource already exists.");

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Conflict, result.ErrorType);
        Assert.Equal("Resource already exists.", result.ErrorMessage);
    }

    [Fact]
    public void Unprocessable_Should_Return_Failure_With_Unprocessable_ErrorType()
    {
        var result = Result.Unprocessable("Cannot process.");

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Unprocessable, result.ErrorType);
        Assert.Equal("Cannot process.", result.ErrorMessage);
    }

    [Fact]
    public void Error_Should_Return_Failure_With_Error_ErrorType()
    {
        var result = Result.Error("Unexpected error.");

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Equal("Unexpected error.", result.ErrorMessage);
    }
}

public class Result_Generic_Tests
{
    [Fact]
    public void Ok_Should_Return_Success_Result_With_Value()
    {
        var result = Result<string>.Ok("hello");

        Assert.True(result.Success);
        Assert.Equal("hello", result.Value);
        Assert.Null(result.ErrorType);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void BadRequest_Should_Return_Failure_With_No_Value()
    {
        var result = Result<string>.BadRequest("Invalid input.");

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Equal("Invalid input.", result.ErrorMessage);
        Assert.Null(result.Value);
    }

    [Fact]
    public void NotFound_Should_Return_Failure_With_NotFound_ErrorType()
    {
        var result = Result<string>.NotFound("Not found.");

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Equal("Not found.", result.ErrorMessage);
    }

    [Fact]
    public void Conflict_Should_Return_Failure_With_Conflict_ErrorType()
    {
        var result = Result<string>.Conflict("Conflict.");

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Conflict, result.ErrorType);
        Assert.Equal("Conflict.", result.ErrorMessage);
    }

    [Fact]
    public void Unprocessable_Should_Return_Failure_With_Unprocessable_ErrorType()
    {
        var result = Result<string>.Unprocessable("Cannot process.");

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Unprocessable, result.ErrorType);
        Assert.Equal("Cannot process.", result.ErrorMessage);
    }

    [Fact]
    public void Error_Should_Return_Failure_With_Error_ErrorType()
    {
        var result = Result<string>.Error("Unexpected error.");

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Equal("Unexpected error.", result.ErrorMessage);
    }
}
