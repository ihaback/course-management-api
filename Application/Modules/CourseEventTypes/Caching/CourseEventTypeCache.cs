using Backend.Application.Common.Caching;
using Backend.Domain.Modules.CourseEventTypes.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.Modules.CourseEventTypes.Caching;

public sealed class CourseEventTypeCache(IMemoryCache cache) : CacheEntityBase<CourseEventType, int>(cache), ICourseEventTypeCache
{
    protected override int GetId(CourseEventType entity) => entity.Id;

    protected override IEnumerable<(string PropertyName, string Value)> GetCachedProperties(CourseEventType entity)
        => [("name", entity.Name)];

    public Task<CourseEventType?> GetByIdAsync(int id, Func<CancellationToken, Task<CourseEventType?>> factory, CancellationToken ct)
        => GetOrCreateByIdAsync(id, factory, ct);

    public Task<CourseEventType?> GetByNameAsync(string name, Func<CancellationToken, Task<CourseEventType?>> factory, CancellationToken ct)
        => GetOrCreateByPropertyNameAsync("name", name, factory, ct);

    public Task<IReadOnlyList<CourseEventType>> GetAllAsync(Func<CancellationToken, Task<IReadOnlyList<CourseEventType>>> factory, CancellationToken ct)
        => GetOrCreateAllAsync(factory, ct);
}
