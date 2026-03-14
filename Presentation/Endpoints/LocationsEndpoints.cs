using Backend.Application.Modules.Locations;
using Backend.Application.Modules.Locations.Inputs;
using Backend.Presentation.API.Models.Location;

namespace Backend.Presentation.API.Endpoints;

public static class LocationsEndpoints
{
    public static RouteGroupBuilder MapLocationsEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/locations")
            .WithTags("Locations");

        group.MapGet("", GetAllLocations).WithName("GetAllLocations");
        group.MapGet("/{id:int}", GetLocationById).WithName("GetLocationById");
        group.MapPost("", CreateLocation).WithName("CreateLocation");
        group.MapPut("/{id:int}", UpdateLocation).WithName("UpdateLocation");
        group.MapDelete("/{id:int}", DeleteLocation).WithName("DeleteLocation");

        return group;
    }

    private static async Task<IResult> GetAllLocations(ILocationService locationService, CancellationToken cancellationToken)
    {
        var response = await locationService.GetAllLocationsAsync(cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> GetLocationById(int id, ILocationService locationService, CancellationToken cancellationToken)
    {
        var response = await locationService.GetLocationByIdAsync(id, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> CreateLocation(CreateLocationRequest request, ILocationService locationService, CancellationToken cancellationToken)
    {
        var input = new CreateLocationInput(request.StreetName, request.PostalCode, request.City);
        var response = await locationService.CreateLocationAsync(input, cancellationToken);
        if (!response.Success)
            return response.ToHttpResult();

        return Results.Created($"/api/locations/{response.Value?.Id}", response);
    }

    private static async Task<IResult> UpdateLocation(int id, UpdateLocationRequest request, ILocationService locationService, CancellationToken cancellationToken)
    {
        var input = new UpdateLocationInput(id, request.StreetName, request.PostalCode, request.City);
        var response = await locationService.UpdateLocationAsync(input, cancellationToken);
        return response.ToHttpResult();
    }

    private static async Task<IResult> DeleteLocation(int id, ILocationService locationService, CancellationToken cancellationToken)
    {
        var response = await locationService.DeleteLocationAsync(id, cancellationToken);
        return response.ToHttpResult();
    }
}



