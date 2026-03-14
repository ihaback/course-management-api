using Backend.Application.Common;
using Backend.Application.Modules.CourseRegistrations.Inputs;
using Backend.Application.Modules.CourseRegistrations.Outputs;
using Backend.Domain.Modules.CourseRegistrations.Models;

namespace Backend.Application.Modules.CourseRegistrations;

public interface ICourseRegistrationService
{
    Task<Result<CourseRegistration>> CreateCourseRegistrationAsync(CreateCourseRegistrationInput courseRegistration, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CourseRegistration>>> GetAllCourseRegistrationsAsync(CancellationToken cancellationToken = default);
    Task<Result<CourseRegistrationDetails>> GetCourseRegistrationByIdAsync(Guid courseRegistrationId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CourseRegistration>>> GetCourseRegistrationsByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CourseRegistration>>> GetCourseRegistrationsByCourseEventIdAsync(Guid courseEventId, CancellationToken cancellationToken = default);
    Task<Result<CourseRegistration>> UpdateCourseRegistrationAsync(UpdateCourseRegistrationInput courseRegistration, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteCourseRegistrationAsync(Guid courseRegistrationId, CancellationToken cancellationToken = default);
}
