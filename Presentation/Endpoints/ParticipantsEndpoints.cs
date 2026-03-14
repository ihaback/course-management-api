using Backend.Application.Modules.Participants;
using Backend.Application.Modules.Participants.Inputs;
using Backend.Presentation.API.Models.Participant;

namespace Backend.Presentation.API.Endpoints;

public static class ParticipantsEndpoints
{
    public static RouteGroupBuilder MapParticipantsEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/participants")
            .WithTags("Participants");

        group.MapGet("", GetAllParticipants).WithName("GetAllParticipants");
        group.MapGet("/{id:guid}", GetParticipantById).WithName("GetParticipantById");
        group.MapPost("", CreateParticipant).WithName("CreateParticipant");
        group.MapPut("/{id:guid}", UpdateParticipant).WithName("UpdateParticipant");
        group.MapDelete("/{id:guid}", DeleteParticipant).WithName("DeleteParticipant");

        return group;
    }

    private static async Task<IResult> GetAllParticipants(IParticipantService participantService, CancellationToken cancellationToken)
    {
        var response = await participantService.GetAllParticipantsAsync(cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> GetParticipantById(Guid id, IParticipantService participantService, CancellationToken cancellationToken)
    {
        var response = await participantService.GetParticipantByIdAsync(id, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> CreateParticipant(CreateParticipantRequest request, IParticipantService participantService, CancellationToken cancellationToken)
    {
        var input = new CreateParticipantInput(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.ContactTypeId);
        var response = await participantService.CreateParticipantAsync(input, cancellationToken);
        if (!response.Success)
            return response.ToHttpResult();

        return Results.Created($"/api/participants/{response.Value?.Id}", response);
    }

    private static async Task<IResult> UpdateParticipant(Guid id, UpdateParticipantRequest request, IParticipantService participantService, CancellationToken cancellationToken)
    {
        var input = new UpdateParticipantInput(id, request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.ContactTypeId);
        var response = await participantService.UpdateParticipantAsync(input, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> DeleteParticipant(Guid id, IParticipantService participantService, CancellationToken cancellationToken)
    {
        var response = await participantService.DeleteParticipantAsync(id, cancellationToken);
        return response.ToHttpResult();
    }
}



