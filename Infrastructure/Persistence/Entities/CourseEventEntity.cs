namespace Backend.Infrastructure.Persistence.Entities
{
    public class CourseEventEntity
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public DateTime EventDate { get; set; }
        public decimal Price { get; set; }
        public int Seats { get; set; }
        public int CourseEventTypeId { get; set; }
        public int VenueTypeId { get; set; }
        public byte[] Concurrency { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ModifiedAtUtc { get; set; }
        public CourseEntity Course { get; set; } = null!;
        public CourseEventTypeEntity CourseEventType { get; set; } = null!;
        public VenueTypeEntity VenueType { get; set; } = null!;
        public virtual ICollection<InstructorEntity> Instructors { get; set; } = [];
        public virtual ICollection<InPlaceLocationEntity> InPlaceLocations { get; set; } = [];
        public virtual ICollection<CourseRegistrationEntity> Registrations { get; set; } = [];
    }
}
