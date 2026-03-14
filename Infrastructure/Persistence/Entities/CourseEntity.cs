namespace Backend.Infrastructure.Persistence.Entities
{
    public class CourseEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int DurationInDays { get; set; }
        public byte[] Concurrency { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ModifiedAtUtc { get; set; }
        public virtual ICollection<CourseEventEntity> CourseEvents { get; set; } = [];
    }
}
