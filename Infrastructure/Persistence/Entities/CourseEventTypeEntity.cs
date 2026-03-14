namespace Backend.Infrastructure.Persistence.Entities
{
    public class CourseEventTypeEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public byte[] Concurrency { get; set; } = null!;
        public virtual ICollection<CourseEventEntity> CourseEvents { get; set; } = [];
    }
}
