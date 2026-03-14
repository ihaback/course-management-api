namespace Backend.Infrastructure.Persistence.Entities
{
    public class ParticipantEntity
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public int ContactTypeId { get; set; }
        public byte[] Concurrency { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ModifiedAtUtc { get; set; }
        public virtual ICollection<CourseRegistrationEntity> CourseRegistrations { get; set; } = [];
        public ParticipantContactTypeEntity ContactType { get; set; } = null!;
    }
}
