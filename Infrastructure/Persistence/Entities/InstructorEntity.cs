namespace Backend.Infrastructure.Persistence.Entities
{
    public class InstructorEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int InstructorRoleId { get; set; }
        public byte[] Concurrency { get; set; } = null!;
        public InstructorRoleEntity? InstructorRole { get; set; }
        public virtual ICollection<CourseEventEntity> CourseEvents { get; set; } = [];
    }
}
