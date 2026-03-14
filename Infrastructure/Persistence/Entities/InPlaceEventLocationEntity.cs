namespace Backend.Infrastructure.Persistence.Entities
{
    public class InPlaceEventLocationEntity
    {
        public Guid CourseEventId { get; set; }
        public int InPlaceLocationId { get; set; }
        public CourseEventEntity CourseEvent { get; set; } = null!;
        public InPlaceLocationEntity InPlaceLocation { get; set; } = null!;
    }
}
