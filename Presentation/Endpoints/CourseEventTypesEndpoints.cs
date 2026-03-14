using Backend.Application.Modules.CourseEventTypes;
using Backend.Application.Modules.CourseEventTypes.Inputs;
using Backend.Presentation.API.Models.CourseEventType;

namespace Backend.Presentation.API.Endpoints;

public static class CourseEventTypesEndpoints
{
    public static RouteGroupBuilder MapCourseEventTypesEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/course-event-types")
            .WithTags("Course event types");

        group.MapGet("", GetAllCourseEventTypes).WithName("GetAllCourseEventTypes");
        group.MapGet("/{id:int}", GetCourseEventTypeById).WithName("GetCourseEventTypeById");
        group.MapPost("", CreateCourseEventType).WithName("CreateCourseEventType");
        group.MapPut("/{id:int}", UpdateCourseEventType).WithName("UpdateCourseEventType");
        group.MapDelete("/{id:int}", DeleteCourseEventType).WithName("DeleteCourseEventType");

        return group;
    }

    private static async Task<IResult> GetAllCourseEventTypes(ICourseEventTypeService service, CancellationToken cancellationToken)
    {
        var response = await service.GetAllCourseEventTypesAsync(cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> GetCourseEventTypeById(int id, ICourseEventTypeService service, CancellationToken cancellationToken)
    {
        var response = await service.GetCourseEventTypeByIdAsync(id, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> CreateCourseEventType(CreateCourseEventTypeRequest request, ICourseEventTypeService service, CancellationToken cancellationToken)
    {
        var input = new CreateCourseEventTypeInput(request.Name);
        var response = await service.CreateCourseEventTypeAsync(input, cancellationToken);
        if (!response.Success)
            return response.ToHttpResult();

        return Results.Created($"/api/course-event-types/{response.Value?.Id}", response);
    }

    private static async Task<IResult> UpdateCourseEventType(int id, UpdateCourseEventTypeRequest request, ICourseEventTypeService service, CancellationToken cancellationToken)
    {
        var input = new UpdateCourseEventTypeInput(id, request.Name);
        var response = await service.UpdateCourseEventTypeAsync(input, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> DeleteCourseEventType(int id, ICourseEventTypeService service, CancellationToken cancellationToken)
    {
        var response = await service.DeleteCourseEventTypeAsync(id, cancellationToken);
        return response.ToHttpResult();
    }
}



