namespace Backend.Infrastructure.Persistence.Entities
{
    public class InPlaceLocationEntity
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public int RoomNumber { get; set; }
        public int Seats { get; set; }
        public byte[] Concurrency { get; set; } = null!;
        public LocationEntity Location { get; set; } = null!;
        public virtual ICollection<CourseEventEntity> CourseEvents { get; set; } = [];
    }
}
