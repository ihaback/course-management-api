using Backend.Application.Common;

namespace Backend.Presentation.API.Endpoints;

public static class ResultHttpExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        if (result.Success) return Results.Ok(result);
        return result.ErrorType switch
        {
            ErrorTypes.NotFound => Results.NotFound(result),
            ErrorTypes.BadRequest => Results.BadRequest(result),
            ErrorTypes.Conflict => Results.Conflict(result),
            ErrorTypes.Unprocessable => Results.UnprocessableEntity(result),
            ErrorTypes.Unauthorized => Results.Json(result, statusCode: 401),
            ErrorTypes.Forbidden => Results.Json(result, statusCode: 403),
            ErrorTypes.Error => Results.Problem(result.ErrorMessage),
            _ => Results.Problem("An unknown error occurred.")
        };
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.Success) return Results.Ok(result);
        return result.ErrorType switch
        {
            ErrorTypes.NotFound => Results.NotFound(result),
            ErrorTypes.BadRequest => Results.BadRequest(result),
            ErrorTypes.Conflict => Results.Conflict(result),
            ErrorTypes.Unprocessable => Results.UnprocessableEntity(result),
            ErrorTypes.Unauthorized => Results.Json(result, statusCode: 401),
            ErrorTypes.Forbidden => Results.Json(result, statusCode: 403),
            ErrorTypes.Error => Results.Problem(result.ErrorMessage),
            _ => Results.Problem("An unknown error occurred.")
        };
    }
}

