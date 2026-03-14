using Backend.Domain.Modules.CourseRegistrations.Models;
using Backend.Domain.Common.Base;

namespace Backend.Domain.Modules.CourseRegistrations.Contracts;

public interface ICourseRegistrationRepository : IRepositoryBase<CourseRegistration, Guid>
{
    Task<IReadOnlyList<CourseRegistration>> GetCourseRegistrationsByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CourseRegistration>> GetCourseRegistrationsByCourseEventIdAsync(Guid courseEventId, CancellationToken cancellationToken);
}

