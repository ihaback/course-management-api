namespace Backend.Infrastructure.Persistence.Entities;

public class VenueTypeEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public byte[] Concurrency { get; set; } = null!;
}
