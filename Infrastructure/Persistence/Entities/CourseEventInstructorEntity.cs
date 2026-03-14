namespace Backend.Infrastructure.Persistence.Entities
{
    public class CourseEventInstructorEntity
    {
        public Guid CourseEventId { get; set; }
        public Guid InstructorId { get; set; }
        public CourseEventEntity CourseEvent { get; set; } = null!;
        public InstructorEntity Instructor { get; set; } = null!;
    }
}
