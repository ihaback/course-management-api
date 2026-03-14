using Backend.Application.Common.Caching;
using Backend.Domain.Modules.CourseRegistrationStatuses.Models;

namespace Backend.Application.Modules.CourseRegistrationStatuses.Caching;

public interface ICourseRegistrationStatusCache : ICacheEntityBase<CourseRegistrationStatus, int>
{
    Task<IReadOnlyList<CourseRegistrationStatus>> GetAllAsync(Func<CancellationToken, Task<IReadOnlyList<CourseRegistrationStatus>>> factory, CancellationToken ct);
    Task<CourseRegistrationStatus?> GetByIdAsync(int id, Func<CancellationToken, Task<CourseRegistrationStatus?>> factory, CancellationToken ct);
}
