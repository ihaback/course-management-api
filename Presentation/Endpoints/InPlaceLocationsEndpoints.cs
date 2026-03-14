using Backend.Application.Modules.InPlaceLocations;
using Backend.Application.Modules.InPlaceLocations.Inputs;
using Backend.Presentation.API.Models.InPlaceLocation;

namespace Backend.Presentation.API.Endpoints;

public static class InPlaceLocationsEndpoints
{
    public static RouteGroupBuilder MapInPlaceLocationsEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/in-place-locations")
            .WithTags("In-place locations");
        var locationsGroup = api.MapGroup("/locations");

        group.MapGet("", GetAllInPlaceLocations).WithName("GetAllInPlaceLocations");
        group.MapGet("/{id:int}", GetInPlaceLocationById).WithName("GetInPlaceLocationById");
        locationsGroup.MapGet("/{locationId:int}/in-place-locations", GetInPlaceLocationsByLocationId).WithName("GetInPlaceLocationsByLocationId");
        group.MapPost("", CreateInPlaceLocation).WithName("CreateInPlaceLocation");
        group.MapPut("/{id:int}", UpdateInPlaceLocation).WithName("UpdateInPlaceLocation");
        group.MapDelete("/{id:int}", DeleteInPlaceLocation).WithName("DeleteInPlaceLocation");

        return group;
    }

    private static async Task<IResult> GetAllInPlaceLocations(IInPlaceLocationService inPlaceLocationService, CancellationToken cancellationToken)
    {
        var response = await inPlaceLocationService.GetAllInPlaceLocationsAsync(cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> GetInPlaceLocationById(int id, IInPlaceLocationService inPlaceLocationService, CancellationToken cancellationToken)
    {
        var response = await inPlaceLocationService.GetInPlaceLocationByIdAsync(id, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> GetInPlaceLocationsByLocationId(int locationId, IInPlaceLocationService inPlaceLocationService, CancellationToken cancellationToken)
    {
        var response = await inPlaceLocationService.GetInPlaceLocationsByLocationIdAsync(locationId, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> CreateInPlaceLocation(CreateInPlaceLocationRequest request, IInPlaceLocationService inPlaceLocationService, CancellationToken cancellationToken)
    {
        var input = new CreateInPlaceLocationInput(request.LocationId, request.RoomNumber, request.Seats);
        var response = await inPlaceLocationService.CreateInPlaceLocationAsync(input, cancellationToken);
        if (!response.Success)
            return response.ToHttpResult();

        return Results.Created($"/api/in-place-locations/{response.Value?.Id}", response);
    }

    private static async Task<IResult> UpdateInPlaceLocation(int id, UpdateInPlaceLocationRequest request, IInPlaceLocationService inPlaceLocationService, CancellationToken cancellationToken)
    {
        var input = new UpdateInPlaceLocationInput(id, request.LocationId, request.RoomNumber, request.Seats);
        var response = await inPlaceLocationService.UpdateInPlaceLocationAsync(input, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> DeleteInPlaceLocation(int id, IInPlaceLocationService inPlaceLocationService, CancellationToken cancellationToken)
    {
        var response = await inPlaceLocationService.DeleteInPlaceLocationAsync(id, cancellationToken);
        return response.ToHttpResult();
    }
}



