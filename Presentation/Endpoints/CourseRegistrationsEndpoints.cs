using Backend.Application.Modules.CourseRegistrations;
using Backend.Application.Modules.CourseRegistrations.Inputs;
using Backend.Presentation.API.Models.CourseRegistration;

namespace Backend.Presentation.API.Endpoints;

public static class CourseRegistrationsEndpoints
{
    public static RouteGroupBuilder MapCourseRegistrationsEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/course-registrations")
            .WithTags("Course registrations");
        var participantsGroup = api.MapGroup("/participants");
        var courseEventsGroup = api.MapGroup("/course-events");

        group.MapGet("", GetAllCourseRegistrations).WithName("GetAllCourseRegistrations");
        group.MapGet("/{id:guid}", GetCourseRegistrationById).WithName("GetCourseRegistrationById");
        participantsGroup.MapGet("/{participantId:guid}/registrations", GetCourseRegistrationsByParticipantId).WithName("GetCourseRegistrationsByParticipantId");
        courseEventsGroup.MapGet("/{courseEventId:guid}/registrations", GetCourseRegistrationsByCourseEventId).WithName("GetCourseRegistrationsByCourseEventId");
        group.MapPost("", CreateCourseRegistration).WithName("CreateCourseRegistration");
        group.MapPut("/{id:guid}", UpdateCourseRegistration).WithName("UpdateCourseRegistration");
        group.MapDelete("/{id:guid}", DeleteCourseRegistration).WithName("DeleteCourseRegistration");

        return group;
    }

    private static async Task<IResult> GetAllCourseRegistrations(ICourseRegistrationService service, CancellationToken cancellationToken)
    {
        var response = await service.GetAllCourseRegistrationsAsync(cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> GetCourseRegistrationById(Guid id, ICourseRegistrationService service, CancellationToken cancellationToken)
    {
        var response = await service.GetCourseRegistrationByIdAsync(id, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> GetCourseRegistrationsByParticipantId(Guid participantId, ICourseRegistrationService service, CancellationToken cancellationToken)
    {
        var response = await service.GetCourseRegistrationsByParticipantIdAsync(participantId, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> GetCourseRegistrationsByCourseEventId(Guid courseEventId, ICourseRegistrationService service, CancellationToken cancellationToken)
    {
        var response = await service.GetCourseRegistrationsByCourseEventIdAsync(courseEventId, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> CreateCourseRegistration(CreateCourseRegistrationRequest request, ICourseRegistrationService service, CancellationToken cancellationToken)
    {
        var input = new CreateCourseRegistrationInput(request.ParticipantId, request.CourseEventId, request.StatusId, request.PaymentMethodId);
        var response = await service.CreateCourseRegistrationAsync(input, cancellationToken);
        if (!response.Success)
            return response.ToHttpResult();

        return Results.Created($"/api/course-registrations/{response.Value?.Id}", response);
    }

    private static async Task<IResult> UpdateCourseRegistration(Guid id, UpdateCourseRegistrationRequest request, ICourseRegistrationService service, CancellationToken cancellationToken)
    {
        var input = new UpdateCourseRegistrationInput(id, request.ParticipantId, request.CourseEventId, request.StatusId, request.PaymentMethodId);
        var response = await service.UpdateCourseRegistrationAsync(input, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> DeleteCourseRegistration(Guid id, ICourseRegistrationService service, CancellationToken cancellationToken)
    {
        var response = await service.DeleteCourseRegistrationAsync(id, cancellationToken);
        return response.ToHttpResult();
    }
}



