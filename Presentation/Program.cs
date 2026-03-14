using Backend.Application.Common;
using Backend.Application.Extensions;
using Backend.Infrastructure.Extensions;
using Backend.Infrastructure.Persistence;
using Backend.Presentation.API.Endpoints;

namespace Backend.Presentation.API;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddCors();
        builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = false);

        builder.Services.AddMemoryCache();

        builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
        builder.Services.AddApplication(builder.Configuration, builder.Environment);

        var app = builder.Build();

        app.UseStatusCodePages(async statusCodeContext =>
        {
            var http = statusCodeContext.HttpContext;
            var response = http.Response;

            if (!response.HasStarted && (response.ContentLength ?? 0) == 0)
            {
                response.ContentType = "application/json";
                Result? payload = response.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => Result.BadRequest("Malformed JSON payload."),
                    StatusCodes.Status404NotFound => Result.NotFound("Resource not found."),
                    StatusCodes.Status409Conflict => Result.Conflict("Conflict."),
                    StatusCodes.Status422UnprocessableEntity => Result.Unprocessable("Unprocessable entity."),
                    _ => null
                };

                if (payload is not null)
                    await response.WriteAsJsonAsync(payload);
            }
        });

        await PersistenceDatabaseInitializer.InitializeAsync(app.Services, app.Environment);

        app.MapOpenApi();
        app.UseHttpsRedirection();
        // NOTE: This allows all origins, headers and methods — only suitable for development.
        // Before deploying to production, replace with a restrictive policy:
        app.UseCors(policy => policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
        app.MapApiEndpoints();

        app.Run();
    }
}
