namespace Backend.Application.Modules.InPlaceLocations.Inputs;

public sealed record CreateInPlaceLocationInput(
    int LocationId,
    int RoomNumber,
    int Seats
);
