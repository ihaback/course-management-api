using System.Text.Json.Serialization;

namespace Backend.Domain.Modules.InPlaceLocations.Models;

public sealed class InPlaceLocation
{
    public int Id { get; }
    public int LocationId { get; private set; }
    public int RoomNumber { get; private set; }
    public int Seats { get; private set; }

    /// <summary>For deserialization only — do not call directly. Use <see cref="Create"/> or <see cref="Reconstitute"/>.</summary>
    [JsonConstructor]
    private InPlaceLocation(int id, int locationId, int roomNumber, int seats)
    {
        if (id < 0)
            throw new ArgumentException("ID must be greater than or equal to zero.", nameof(id));

        Id = id;
        SetValues(locationId, roomNumber, seats);
    }

    public static InPlaceLocation Create(int locationId, int roomNumber, int seats)
        => new(0, locationId, roomNumber, seats);

    public static InPlaceLocation Reconstitute(int id, int locationId, int roomNumber, int seats)
        => new(id, locationId, roomNumber, seats);

    public void Update(int locationId, int roomNumber, int seats)
    {
        SetValues(locationId, roomNumber, seats);
    }

    private void SetValues(int locationId, int roomNumber, int seats)
    {
        if (locationId <= 0)
            throw new ArgumentException("Location ID must be greater than zero.", nameof(locationId));

        if (roomNumber <= 0)
            throw new ArgumentException("Room number must be greater than zero.", nameof(roomNumber));

        if (seats <= 0)
            throw new ArgumentException("Seats must be greater than zero.", nameof(seats));

        LocationId = locationId;
        RoomNumber = roomNumber;
        Seats = seats;
    }
}
