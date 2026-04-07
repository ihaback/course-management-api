using Backend.Application.Modules.CourseEventTypes;
using Backend.Application.Modules.CourseEventTypes.Caching;
using Backend.Application.Modules.CourseEventTypes.Inputs;
using Backend.Domain.Modules.CourseEventTypes.Contracts;
using Backend.Domain.Modules.CourseEventTypes.Models;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace Backend.Tests.Unit.Application.Modules.CourseEventTypes;

/// <summary>
/// Cache-integration tests for <see cref="CourseEventTypeService"/> using a real
/// <see cref="CourseEventTypeCache"/>. Verifies that name-based lookups are cached
/// and that updates evict the old name key.
/// </summary>
public class CourseEventTypeService_CacheIntegration_Tests
{
    private static (CourseEventTypeService service, CourseEventTypeCache cache, ICourseEventTypeRepository repo) Create()
    {
        var repo = Substitute.For<ICourseEventTypeRepository>();
        var cache = new CourseEventTypeCache(new MemoryCache(new MemoryCacheOptions()));
        return (new CourseEventTypeService(cache, repo), cache, repo);
    }

    [Fact]
    public async Task GetCourseEventTypeByTypeNameAsync_HitsCache_OnSecondCall_RepoCalledOnce()
    {
        var (service, _, repo) = Create();
        var entity = CourseEventType.Reconstitute(1, "Online");

        repo.GetCourseEventTypeByTypeNameAsync("Online", Arg.Any<CancellationToken>()).Returns(entity);

        var first = await service.GetCourseEventTypeByTypeNameAsync("Online", CancellationToken.None);
        var second = await service.GetCourseEventTypeByTypeNameAsync("Online", CancellationToken.None);

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.Equal("Online", second.Value!.Name);
        await repo.Received(1).GetCourseEventTypeByTypeNameAsync("Online", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCourseEventTypeAsync_EvictsOldNameKey_AndCachesNewName()
    {
        var (service, cache, repo) = Create();
        var original = CourseEventType.Reconstitute(1, "Webinar");
        var renamed = CourseEventType.Reconstitute(1, "Online");

        cache.SetEntity(original);

        repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(original);
        repo.UpdateAsync(1, Arg.Any<CourseEventType>(), Arg.Any<CancellationToken>()).Returns(renamed);

        await service.UpdateCourseEventTypeAsync(new UpdateCourseEventTypeInput(1, "Online"), CancellationToken.None);

        var oldCalls = 0;
        await cache.GetByNameAsync("Webinar",
            _ => { oldCalls++; return Task.FromResult<CourseEventType?>(renamed); },
            CancellationToken.None);

        var newCalls = 0;
        await cache.GetByNameAsync("Online",
            _ => { newCalls++; return Task.FromResult<CourseEventType?>(renamed); },
            CancellationToken.None);

        Assert.Equal(1, oldCalls);
        Assert.Equal(0, newCalls);
    }
}
