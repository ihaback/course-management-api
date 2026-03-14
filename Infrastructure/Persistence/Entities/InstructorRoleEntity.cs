namespace Backend.Infrastructure.Persistence.Entities;

public class InstructorRoleEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public byte[] Concurrency { get; set; } = null!;

    public virtual ICollection<InstructorEntity> Instructors { get; set; } = [];
}
