using Backend.Application.Modules.CourseRegistrationStatuses;
using Backend.Application.Modules.CourseRegistrationStatuses.Caching;
using Backend.Application.Modules.CourseRegistrationStatuses.Inputs;
using Backend.Domain.Modules.CourseRegistrationStatuses.Contracts;
using Backend.Domain.Modules.CourseRegistrationStatuses.Models;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace Backend.Tests.Unit.Application.Modules.CourseRegistrations;

/// <summary>
/// Cache-integration tests for <see cref="CourseRegistrationStatusService"/> using a real
/// <see cref="CourseRegistrationStatusCache"/>. Verifies that name-based lookups are cached
/// and that updates evict the old name key.
/// </summary>
public class CourseRegistrationStatusService_CacheIntegration_Tests
{
    private static (CourseRegistrationStatusService service, CourseRegistrationStatusCache cache, ICourseRegistrationStatusRepository repo) Create()
    {
        var repo = Substitute.For<ICourseRegistrationStatusRepository>();
        var cache = new CourseRegistrationStatusCache(new MemoryCache(new MemoryCacheOptions()));
        return (new CourseRegistrationStatusService(cache, repo), cache, repo);
    }

    [Fact]
    public async Task GetCourseRegistrationStatusByNameAsync_HitsCache_OnSecondCall_RepoCalledOnce()
    {
        var (service, _, repo) = Create();
        var entity = CourseRegistrationStatus.Reconstitute(1, "Paid");

        repo.GetCourseRegistrationStatusByNameAsync("Paid", Arg.Any<CancellationToken>()).Returns(entity);

        var first = await service.GetCourseRegistrationStatusByNameAsync("Paid", CancellationToken.None);
        var second = await service.GetCourseRegistrationStatusByNameAsync("Paid", CancellationToken.None);

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.Equal("Paid", second.Value!.Name);
        // Repo must only be called on the first (cache-miss) request
        await repo.Received(1).GetCourseRegistrationStatusByNameAsync("Paid", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCourseRegistrationStatusAsync_EvictsOldNameKey_AndCachesNewName()
    {
        var (service, cache, repo) = Create();
        var original = CourseRegistrationStatus.Reconstitute(1, "Pending");
        var renamed = CourseRegistrationStatus.Reconstitute(1, "Awaiting Payment");

        cache.SetEntity(original); // seed cache with old name

        repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(original);
        repo.UpdateAsync(1, Arg.Any<CourseRegistrationStatus>(), Arg.Any<CancellationToken>()).Returns(renamed);

        await service.UpdateCourseRegistrationStatusAsync(
            new UpdateCourseRegistrationStatusInput(1, "Awaiting Payment"), CancellationToken.None);

        // Old name → cache miss
        var oldCalls = 0;
        await cache.GetByNameAsync("Pending",
            _ => { oldCalls++; return Task.FromResult<CourseRegistrationStatus?>(renamed); },
            CancellationToken.None);

        // New name → cache hit
        var newCalls = 0;
        await cache.GetByNameAsync("Awaiting Payment",
            _ => { newCalls++; return Task.FromResult<CourseRegistrationStatus?>(renamed); },
            CancellationToken.None);

        Assert.Equal(1, oldCalls); // old name was evicted
        Assert.Equal(0, newCalls); // new name was cached
    }
}
