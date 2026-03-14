using System.ComponentModel.DataAnnotations;
using Backend.Application.Common;

namespace Backend.Presentation.API.Endpoints;

public static class RequestValidationFilter
{
    public static EndpointFilterDelegate Factory(EndpointFilterFactoryContext context, EndpointFilterDelegate next)
    {
        var modelArgumentIndexes = context.MethodInfo
            .GetParameters()
            .Select((parameter, index) => (parameter, index))
            .Where(x => x.parameter.ParameterType.Namespace?.StartsWith("Backend.Presentation.API.Models", StringComparison.Ordinal) == true)
            .Select(x => x.index)
            .ToArray();

        if (modelArgumentIndexes.Length == 0)
            return next;

        return async invocationContext =>
        {
            foreach (var index in modelArgumentIndexes)
            {
                var argument = invocationContext.Arguments[index];
                if (argument is null)
                    continue;

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(argument);
                var isValid = Validator.TryValidateObject(argument, validationContext, validationResults, true);

                if (!isValid)
                {
                    var message = string.Join(
                        "; ",
                        validationResults
                            .Select(result => result.ErrorMessage)
                            .Where(error => !string.IsNullOrWhiteSpace(error)));

                    var errorMessage = string.IsNullOrWhiteSpace(message) ? "Validation failed." : message;
                    return Result.BadRequest(errorMessage).ToHttpResult();
                }
            }

            return await next(invocationContext);
        };
    }
}
