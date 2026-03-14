namespace Backend.Application.Modules.InPlaceLocations.Inputs;

public sealed record UpdateInPlaceLocationInput(
    int Id,
    int LocationId,
    int RoomNumber,
    int Seats
);
