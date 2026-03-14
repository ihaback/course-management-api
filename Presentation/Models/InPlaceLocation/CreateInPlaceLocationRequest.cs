using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.InPlaceLocation;

public sealed record CreateInPlaceLocationRequest
{
    [Range(1, int.MaxValue)]
    public int LocationId { get; init; }

    [Range(1, int.MaxValue)]
    public int RoomNumber { get; init; }

    [Range(1, int.MaxValue)]
    public int Seats { get; init; }
}
