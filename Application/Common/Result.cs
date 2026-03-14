namespace Backend.Application.Common;

public enum ErrorTypes
{
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    Unprocessable = 422,
    Error = 500
}

public sealed record Result(bool Success, ErrorTypes? ErrorType = null, string? ErrorMessage = null)
{
    public static Result Ok() => new(true);
    public static Result BadRequest(string message) => new(false, ErrorTypes.BadRequest, message);
    public static Result Unauthorized(string message) => new(false, ErrorTypes.Unauthorized, message);
    public static Result Forbidden(string message) => new(false, ErrorTypes.Forbidden, message);
    public static Result NotFound(string message) => new(false, ErrorTypes.NotFound, message);
    public static Result Conflict(string message) => new(false, ErrorTypes.Conflict, message);
    public static Result Unprocessable(string message) => new(false, ErrorTypes.Unprocessable, message);
    public static Result Error(string message = "An unexpected error occurred.") => new(false, ErrorTypes.Error, message);
}

public sealed record Result<T>(bool Success, T? Value = default, ErrorTypes? ErrorType = null, string? ErrorMessage = null)
{
    public static Result<T> Ok(T value) => new(true, value);
    public static Result<T> BadRequest(string message) => new(false, default, ErrorTypes.BadRequest, message);
    public static Result<T> Unauthorized(string message) => new(false, default, ErrorTypes.Unauthorized, message);
    public static Result<T> Forbidden(string message) => new(false, default, ErrorTypes.Forbidden, message);
    public static Result<T> NotFound(string message) => new(false, default, ErrorTypes.NotFound, message);
    public static Result<T> Conflict(string message) => new(false, default, ErrorTypes.Conflict, message);
    public static Result<T> Unprocessable(string message) => new(false, default, ErrorTypes.Unprocessable, message);
    public static Result<T> Error(string message = "An unexpected error occurred.") => new(false, default, ErrorTypes.Error, message);
}
