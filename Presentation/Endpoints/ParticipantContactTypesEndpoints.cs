using Backend.Application.Modules.ParticipantContactTypes;
using Backend.Application.Modules.ParticipantContactTypes.Inputs;
using Backend.Presentation.API.Models.ParticipantContactType;

namespace Backend.Presentation.API.Endpoints;

public static class ParticipantContactTypesEndpoints
{
    public static RouteGroupBuilder MapParticipantContactTypesEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/participant-contact-types")
            .WithTags("Participant contact types");

        group.MapGet("", GetParticipantContactTypes).WithName("GetParticipantContactTypes");
        group.MapGet("/{id:int}", GetParticipantContactTypeById).WithName("GetParticipantContactTypeById");
        group.MapGet("/by-name/{name}", GetParticipantContactTypeByName).WithName("GetParticipantContactTypeByName");
        group.MapPost("", CreateParticipantContactType).WithName("CreateParticipantContactType");
        group.MapPut("/{id:int}", UpdateParticipantContactType).WithName("UpdateParticipantContactType");
        group.MapDelete("/{id:int}", DeleteParticipantContactType).WithName("DeleteParticipantContactType");

        return group;
    }

    private static async Task<IResult> GetParticipantContactTypes(IParticipantContactTypeService service, CancellationToken cancellationToken)
    {
        var response = await service.GetAllParticipantContactTypesAsync(cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> GetParticipantContactTypeById(int id, IParticipantContactTypeService service, CancellationToken cancellationToken)
    {
        var response = await service.GetParticipantContactTypeByIdAsync(id, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> GetParticipantContactTypeByName(string name, IParticipantContactTypeService service, CancellationToken cancellationToken)
    {
        var response = await service.GetParticipantContactTypeByNameAsync(name, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> CreateParticipantContactType(CreateParticipantContactTypeRequest request, IParticipantContactTypeService service, CancellationToken cancellationToken)
    {
        var input = new CreateParticipantContactTypeInput(request.Name);
        var response = await service.CreateParticipantContactTypeAsync(input, cancellationToken);
        if (!response.Success)
            return response.ToHttpResult();

        return Results.Created($"/api/participant-contact-types/{response.Value?.Id}", response);
    }

    private static async Task<IResult> UpdateParticipantContactType(int id, UpdateParticipantContactTypeRequest request, IParticipantContactTypeService service, CancellationToken cancellationToken)
    {
        var updateInput = new UpdateParticipantContactTypeInput(id, request.Name);
        var response = await service.UpdateParticipantContactTypeAsync(updateInput, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> DeleteParticipantContactType(int id, IParticipantContactTypeService service, CancellationToken cancellationToken)
    {
        var response = await service.DeleteParticipantContactTypeAsync(id, cancellationToken);
        return response.ToHttpResult();
    }
}



