namespace Backend.Infrastructure.Persistence.Entities
{
    public class CourseRegistrationEntity
    {
        public Guid Id { get; set; }
        public Guid ParticipantId { get; set; }
        public Guid CourseEventId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int CourseRegistrationStatusId { get; set; }
        public int PaymentMethodId { get; set; }
        public byte[] Concurrency { get; set; } = null!;
        public DateTime ModifiedAtUtc { get; set; }
        public ParticipantEntity Participant { get; set; } = null!;
        public CourseEventEntity CourseEvent { get; set; } = null!;
        public CourseRegistrationStatusEntity CourseRegistrationStatus { get; set; } = null!;
        public PaymentMethodEntity PaymentMethod { get; set; } = null!;
    }
}

