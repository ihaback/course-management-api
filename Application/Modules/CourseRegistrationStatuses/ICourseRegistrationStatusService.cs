using Backend.Application.Common;
using Backend.Application.Modules.CourseRegistrationStatuses.Inputs;
using Backend.Domain.Modules.CourseRegistrationStatuses.Models;

namespace Backend.Application.Modules.CourseRegistrationStatuses;

public interface ICourseRegistrationStatusService
{
    Task<Result<IReadOnlyList<CourseRegistrationStatus>>> GetAllCourseRegistrationStatusesAsync(CancellationToken cancellationToken = default);
    Task<Result<CourseRegistrationStatus>> GetCourseRegistrationStatusByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<CourseRegistrationStatus>> GetCourseRegistrationStatusByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Result<CourseRegistrationStatus>> CreateCourseRegistrationStatusAsync(CreateCourseRegistrationStatusInput input, CancellationToken cancellationToken = default);
    Task<Result<CourseRegistrationStatus>> UpdateCourseRegistrationStatusAsync(UpdateCourseRegistrationStatusInput input, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteCourseRegistrationStatusAsync(int id, CancellationToken cancellationToken = default);
}
