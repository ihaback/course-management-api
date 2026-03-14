using Backend.Application.Common.Caching;
using Backend.Domain.Modules.CourseEventTypes.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.Modules.CourseEventTypes.Caching;

public sealed class CourseEventTypeCache(IMemoryCache cache) : CacheEntityBase<CourseEventType, int>(cache), ICourseEventTypeCache
{
    protected override int GetId(CourseEventType entity) => entity.Id;

    public Task<CourseEventType?> GetByIdAsync(int id, Func<CancellationToken, Task<CourseEventType?>> factory, CancellationToken ct)
        => GetOrCreateByIdAsync(id, factory, ct);

    public Task<IReadOnlyList<CourseEventType>> GetAllAsync(Func<CancellationToken, Task<IReadOnlyList<CourseEventType>>> factory, CancellationToken ct)
        => GetOrCreateAllAsync(factory, ct);
}
