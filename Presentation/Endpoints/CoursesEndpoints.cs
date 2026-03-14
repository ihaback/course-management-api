using Backend.Application.Modules.Courses;
using Backend.Application.Modules.Courses.Inputs;
using Backend.Presentation.API.Models.Course;

namespace Backend.Presentation.API.Endpoints;

public static class CoursesEndpoints
{
    public static RouteGroupBuilder MapCoursesEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/courses")
            .WithTags("Courses");

        group.MapGet("", GetAllCourses).WithName("GetAllCourses");
        group.MapGet("/{id:guid}", GetCourseById).WithName("GetCourseById");
        group.MapPost("", CreateCourse).WithName("CreateCourse");
        group.MapPut("/{id:guid}", UpdateCourse).WithName("UpdateCourse");
        group.MapDelete("/{id:guid}", DeleteCourse).WithName("DeleteCourse");

        return group;
    }

    private static async Task<IResult> GetAllCourses(ICourseService courseService, CancellationToken cancellationToken)
    {
        var response = await courseService.GetAllCoursesAsync(cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> GetCourseById(Guid id, ICourseService courseService, CancellationToken cancellationToken)
    {
        var response = await courseService.GetCourseByIdAsync(id, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> CreateCourse(CreateCourseRequest request, ICourseService courseService, CancellationToken cancellationToken)
    {
        var input = new CreateCourseInput(request.Title, request.Description, request.DurationInDays);
        var response = await courseService.CreateCourseAsync(input, cancellationToken);
        if (!response.Success)
            return response.ToHttpResult();

        return Results.Created($"/api/courses/{response.Value?.Id}", response);
    }

    private static async Task<IResult> UpdateCourse(Guid id, UpdateCourseRequest request, ICourseService courseService, CancellationToken cancellationToken)
    {
        var input = new UpdateCourseInput(id, request.Title, request.Description, request.DurationInDays);
        var response = await courseService.UpdateCourseAsync(input, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> DeleteCourse(Guid id, ICourseService courseService, CancellationToken cancellationToken)
    {
        var response = await courseService.DeleteCourseAsync(id, cancellationToken);
        return response.ToHttpResult();
    }
}



