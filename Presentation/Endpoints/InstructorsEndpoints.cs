using Backend.Application.Modules.Instructors;
using Backend.Application.Modules.Instructors.Inputs;
using Backend.Presentation.API.Models.Instructor;

namespace Backend.Presentation.API.Endpoints;

public static class InstructorsEndpoints
{
    public static RouteGroupBuilder MapInstructorsEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/instructors")
            .WithTags("Instructors");

        group.MapGet("", GetAllInstructors).WithName("GetAllInstructors");
        group.MapGet("/{id:guid}", GetInstructorById).WithName("GetInstructorById");
        group.MapPost("", CreateInstructor).WithName("CreateInstructor");
        group.MapPut("/{id:guid}", UpdateInstructor).WithName("UpdateInstructor");
        group.MapDelete("/{id:guid}", DeleteInstructor).WithName("DeleteInstructor");

        return group;
    }

    private static async Task<IResult> GetAllInstructors(IInstructorService instructorService, CancellationToken cancellationToken)
    {
        var response = await instructorService.GetAllInstructorsAsync(cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> GetInstructorById(Guid id, IInstructorService instructorService, CancellationToken cancellationToken)
    {
        var response = await instructorService.GetInstructorByIdAsync(id, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> CreateInstructor(CreateInstructorRequest request, IInstructorService instructorService, CancellationToken cancellationToken)
    {
        var input = new CreateInstructorInput(request.Name, request.InstructorRoleId);
        var response = await instructorService.CreateInstructorAsync(input, cancellationToken);
        if (!response.Success)
            return response.ToHttpResult();

        return Results.Created($"/api/instructors/{response.Value?.Id}", response);
    }

    private static async Task<IResult> UpdateInstructor(Guid id, UpdateInstructorRequest request, IInstructorService instructorService, CancellationToken cancellationToken)
    {
        var input = new UpdateInstructorInput(id, request.Name, request.InstructorRoleId);
        var response = await instructorService.UpdateInstructorAsync(input, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> DeleteInstructor(Guid id, IInstructorService instructorService, CancellationToken cancellationToken)
    {
        var response = await instructorService.DeleteInstructorAsync(id, cancellationToken);
        return response.ToHttpResult();
    }
}



