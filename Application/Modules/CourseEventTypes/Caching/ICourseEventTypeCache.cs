using Backend.Application.Common.Caching;
using Backend.Domain.Modules.CourseEventTypes.Models;

namespace Backend.Application.Modules.CourseEventTypes.Caching;

public interface ICourseEventTypeCache : ICacheEntityBase<CourseEventType, int>
{
    Task<IReadOnlyList<CourseEventType>> GetAllAsync(Func<CancellationToken, Task<IReadOnlyList<CourseEventType>>> factory, CancellationToken ct);
    Task<CourseEventType?> GetByIdAsync(int id, Func<CancellationToken, Task<CourseEventType?>> factory, CancellationToken ct);
}
