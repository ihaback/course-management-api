namespace Backend.Infrastructure.Persistence.Entities
{
    public class LocationEntity
    {
        public int Id { get; set; }
        public string StreetName { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string City { get; set; } = null!;
        public byte[] Concurrency { get; set; } = null!;
        public virtual ICollection<InPlaceLocationEntity> InPlaceLocations { get; set; } = [];
    }
}
