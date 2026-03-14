using Backend.Application.Common.Caching;
using Backend.Domain.Modules.CourseRegistrationStatuses.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.Modules.CourseRegistrationStatuses.Caching;

public sealed class CourseRegistrationStatusCache(IMemoryCache cache) : CacheEntityBase<CourseRegistrationStatus, int>(cache), ICourseRegistrationStatusCache
{
    protected override int GetId(CourseRegistrationStatus entity) => entity.Id;

    public Task<CourseRegistrationStatus?> GetByIdAsync(int id, Func<CancellationToken, Task<CourseRegistrationStatus?>> factory, CancellationToken ct)
        => GetOrCreateByIdAsync(id, factory, ct);

    public Task<IReadOnlyList<CourseRegistrationStatus>> GetAllAsync(Func<CancellationToken, Task<IReadOnlyList<CourseRegistrationStatus>>> factory, CancellationToken ct)
        => GetOrCreateAllAsync(factory, ct);
}
