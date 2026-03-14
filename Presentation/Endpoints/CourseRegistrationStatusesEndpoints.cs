using Backend.Application.Modules.CourseRegistrationStatuses;
using Backend.Application.Modules.CourseRegistrationStatuses.Inputs;
using Backend.Presentation.API.Models.CourseRegistrationStatus;

namespace Backend.Presentation.API.Endpoints;

public static class CourseRegistrationStatusesEndpoints
{
    public static RouteGroupBuilder MapCourseRegistrationStatusesEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/course-registration-statuses")
            .WithTags("Course registration statuses");

        group.MapGet("", GetCourseRegistrationStatuses).WithName("GetCourseRegistrationStatuses");
        group.MapGet("/{id:int}", GetCourseRegistrationStatusById).WithName("GetCourseRegistrationStatusById");
        group.MapPost("", CreateCourseRegistrationStatus).WithName("CreateCourseRegistrationStatus");
        group.MapPut("/{id:int}", UpdateCourseRegistrationStatus).WithName("UpdateCourseRegistrationStatus");
        group.MapDelete("/{id:int}", DeleteCourseRegistrationStatus).WithName("DeleteCourseRegistrationStatus");

        return group;
    }

    private static async Task<IResult> GetCourseRegistrationStatuses(ICourseRegistrationStatusService statusService, CancellationToken cancellationToken)
    {
        var response = await statusService.GetAllCourseRegistrationStatusesAsync(cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> GetCourseRegistrationStatusById(int id, ICourseRegistrationStatusService statusService, CancellationToken cancellationToken)
    {
        var response = await statusService.GetCourseRegistrationStatusByIdAsync(id, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> CreateCourseRegistrationStatus(CreateCourseRegistrationStatusRequest request, ICourseRegistrationStatusService statusService, CancellationToken cancellationToken)
    {
        var input = new CreateCourseRegistrationStatusInput(request.Name);
        var response = await statusService.CreateCourseRegistrationStatusAsync(input, cancellationToken);
        if (!response.Success)
            return response.ToHttpResult();

        return Results.Created($"/api/course-registration-statuses/{response.Value?.Id}", response);
    }

    private static async Task<IResult> UpdateCourseRegistrationStatus(int id, UpdateCourseRegistrationStatusRequest request, ICourseRegistrationStatusService statusService, CancellationToken cancellationToken)
    {
        var updateInput = new UpdateCourseRegistrationStatusInput(id, request.Name);
        var response = await statusService.UpdateCourseRegistrationStatusAsync(updateInput, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> DeleteCourseRegistrationStatus(int id, ICourseRegistrationStatusService statusService, CancellationToken cancellationToken)
    {
        var response = await statusService.DeleteCourseRegistrationStatusAsync(id, cancellationToken);
        return response.ToHttpResult();
    }
}



