using Backend.Application.Common;
using Backend.Application.Modules.CourseRegistrationStatuses;
using Backend.Application.Modules.CourseRegistrationStatuses.Caching;
using Backend.Application.Modules.CourseRegistrationStatuses.Inputs;
using Backend.Domain.Modules.CourseRegistrationStatuses.Contracts;
using Backend.Domain.Modules.CourseRegistrationStatuses.Models;
using NSubstitute;

namespace Backend.Tests.Unit.Application.Modules.CourseRegistrations;

public class CourseRegistrationStatusService_Tests
{
    private static CourseRegistrationStatusService CreateService(
        out ICourseRegistrationStatusRepository repo,
        out ICourseRegistrationStatusCache cache)
    {
        repo = Substitute.For<ICourseRegistrationStatusRepository>();
        cache = Substitute.For<ICourseRegistrationStatusCache>();

        cache.GetAllAsync(Arg.Any<Func<CancellationToken, Task<IReadOnlyList<CourseRegistrationStatus>>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<CancellationToken, Task<IReadOnlyList<CourseRegistrationStatus>>>>()(ci.Arg<CancellationToken>())!);

        cache.GetByIdAsync(Arg.Any<int>(), Arg.Any<Func<CancellationToken, Task<CourseRegistrationStatus?>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<CancellationToken, Task<CourseRegistrationStatus?>>>()(ci.Arg<CancellationToken>()));

        cache.GetByNameAsync(Arg.Any<string>(), Arg.Any<Func<CancellationToken, Task<CourseRegistrationStatus?>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<CancellationToken, Task<CourseRegistrationStatus?>>>()(ci.Arg<CancellationToken>()));

        return new CourseRegistrationStatusService(cache, repo);
    }

    [Fact]
    public async Task GetAll_Should_Return_Success_With_Data()
    {
        var service = CreateService(out var repo, out var cache);
        repo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CourseRegistrationStatus> { CourseRegistrationStatus.Reconstitute(1, "Paid"), CourseRegistrationStatus.Reconstitute(0, "Pending") });

        Result<IReadOnlyList<CourseRegistrationStatus>> result = await service.GetAllCourseRegistrationStatusesAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.Equal(2, result.Value?.Count());
        await repo.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        await cache.Received(1).GetAllAsync(
            Arg.Any<Func<CancellationToken, Task<IReadOnlyList<CourseRegistrationStatus>>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAll_Should_Return_Success_NoData()
    {
        var service = CreateService(out var repo, out _);
        repo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CourseRegistrationStatus>());

        var result = await service.GetAllCourseRegistrationStatusesAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAll_Should_Handle_Exception()
    {
        var service = CreateService(out var repo, out _);
        repo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<CourseRegistrationStatus>>(new Exception("db failure")));

        var result = await service.GetAllCourseRegistrationStatusesAsync();

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Contains("An error occurred", result.ErrorMessage);
    }

    [Fact]
    public async Task GetById_Should_Return_400_When_Id_Negative()
    {
        var service = CreateService(out _, out _);

        var result = await service.GetCourseRegistrationStatusByIdAsync(-1);

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Contains("Id must be zero or positive", result.ErrorMessage);
    }

    [Fact]
    public async Task GetById_Should_Return_404_When_NotFound()
    {
        var service = CreateService(out var repo, out var cache);
        repo.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns((CourseRegistrationStatus?)null);

        var result = await service.GetCourseRegistrationStatusByIdAsync(5);

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        await cache.Received(1).GetByIdAsync(
            5,
            Arg.Any<Func<CancellationToken, Task<CourseRegistrationStatus?>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_Should_Return_Status_When_Found()
    {
        var service = CreateService(out var repo, out var cache);
        repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(CourseRegistrationStatus.Reconstitute(1, "Paid"));

        var result = await service.GetCourseRegistrationStatusByIdAsync(1);

        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("Paid", result.Value.Name);
        await cache.Received(1).GetByIdAsync(
            1,
            Arg.Any<Func<CancellationToken, Task<CourseRegistrationStatus?>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByName_Should_Return_400_When_Name_Empty()
    {
        var service = CreateService(out _, out _);

        var result = await service.GetCourseRegistrationStatusByNameAsync(" ");

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Contains("Name is required", result.ErrorMessage);
    }

    [Fact]
    public async Task GetByName_Should_Return_404_When_NotFound()
    {
        var service = CreateService(out var repo, out var cache);
        repo.GetCourseRegistrationStatusByNameAsync("Unknown", Arg.Any<CancellationToken>())
            .Returns((CourseRegistrationStatus?)null);

        var result = await service.GetCourseRegistrationStatusByNameAsync("Unknown");

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        await repo.Received(1).GetCourseRegistrationStatusByNameAsync("Unknown", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByName_Should_Return_Status_When_Found()
    {
        var service = CreateService(out var repo, out var cache);
        repo.GetCourseRegistrationStatusByNameAsync("Paid", Arg.Any<CancellationToken>())
            .Returns(CourseRegistrationStatus.Reconstitute(1, "Paid"));

        var result = await service.GetCourseRegistrationStatusByNameAsync("Paid");

        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("Paid", result.Value.Name);
        await repo.Received(1).GetCourseRegistrationStatusByNameAsync("Paid", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByName_Should_Return_500_When_Repository_Throws()
    {
        var service = CreateService(out var repo, out _);
        repo.GetCourseRegistrationStatusByNameAsync("Paid", Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CourseRegistrationStatus?>(new Exception("db failure")));

        var result = await service.GetCourseRegistrationStatusByNameAsync("Paid");

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Contains("An error occurred while retrieving the course registration status", result.ErrorMessage);
    }

    [Fact]
    public async Task Delete_Should_Return_400_For_Negative_Id()
    {
        var service = CreateService(out _, out _);

        var result = await service.DeleteCourseRegistrationStatusAsync(-2);

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
    }

    [Fact]
    public async Task Delete_Should_Return_404_When_NotFound()
    {
        var service = CreateService(out var repo, out var cache);
        repo.GetByIdAsync(3, Arg.Any<CancellationToken>()).Returns((CourseRegistrationStatus?)null);

        var result = await service.DeleteCourseRegistrationStatusAsync(3);

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        cache.DidNotReceive().ResetEntity(Arg.Any<CourseRegistrationStatus>());
    }

    [Fact]
    public async Task Delete_Should_Return_409_When_InUse()
    {
        var service = CreateService(out var repo, out var cache);
        repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(CourseRegistrationStatus.Reconstitute(1, "Paid"));
        repo.IsInUseAsync(1, Arg.Any<CancellationToken>()).Returns(true);

        var result = await service.DeleteCourseRegistrationStatusAsync(1);

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Conflict, result.ErrorType);
        cache.DidNotReceive().ResetEntity(Arg.Any<CourseRegistrationStatus>());
    }

    [Fact]
    public async Task Delete_Should_Return_200_For_Valid_NotInUse()
    {
        var service = CreateService(out var repo, out var cache);
        repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(CourseRegistrationStatus.Reconstitute(1, "Paid"));
        repo.IsInUseAsync(1, Arg.Any<CancellationToken>()).Returns(false);
        repo.RemoveAsync(1, Arg.Any<CancellationToken>()).Returns(true);

        var result = await service.DeleteCourseRegistrationStatusAsync(1);

        Assert.True(result.Success);
        Assert.Null(result.ErrorType);        cache.Received(1).ResetEntity(Arg.Is<CourseRegistrationStatus>(s => s.Id == 1));
    }

    [Fact]
    public async Task Delete_Should_Return_500_When_Delete_Returns_False()
    {
        var service = CreateService(out var repo, out var cache);
        repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(CourseRegistrationStatus.Reconstitute(1, "Paid"));
        repo.IsInUseAsync(1, Arg.Any<CancellationToken>()).Returns(false);
        repo.RemoveAsync(1, Arg.Any<CancellationToken>()).Returns(false);

        var result = await service.DeleteCourseRegistrationStatusAsync(1);

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);        cache.DidNotReceive().ResetEntity(Arg.Any<CourseRegistrationStatus>());
    }

    [Fact]
    public async Task Delete_Should_Return_500_When_Repository_Throws()
    {
        var service = CreateService(out var repo, out var cache);
        repo.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(CourseRegistrationStatus.Reconstitute(1, "Paid"));
        repo.IsInUseAsync(1, Arg.Any<CancellationToken>()).Returns(false);
        repo.RemoveAsync(1, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("db failure")));

        var result = await service.DeleteCourseRegistrationStatusAsync(1);

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);        cache.DidNotReceive().SetEntity(Arg.Any<CourseRegistrationStatus>());
    }

    [Fact]
    public async Task Create_Should_Return_400_When_Name_Empty()
    {
        var service = CreateService(out _, out _);

        var result = await service.CreateCourseRegistrationStatusAsync(new CreateCourseRegistrationStatusInput("   "));

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
    }

    [Fact]
    public async Task Create_Should_Return_201_When_Name_Valid()
    {
        var service = CreateService(out _, out var cache);
        var repo = Substitute.For<ICourseRegistrationStatusRepository>();
        repo.AddAsync(Arg.Any<CourseRegistrationStatus>(), Arg.Any<CancellationToken>())
            .Returns(CourseRegistrationStatus.Reconstitute(4, "New"));
        service = new CourseRegistrationStatusService(cache, repo);

        var result = await service.CreateCourseRegistrationStatusAsync(new CreateCourseRegistrationStatusInput("New"));

        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(4, result.Value.Id);
        cache.Received(1).ResetEntity(Arg.Is<CourseRegistrationStatus>(s => s.Id == 4));
        cache.Received(1).SetEntity(Arg.Is<CourseRegistrationStatus>(s => s.Id == 4));
    }

    [Fact]
    public async Task Create_Should_Return_400_When_Name_Already_Exists()
    {
        var service = CreateService(out var repo, out var cache);
        repo.GetCourseRegistrationStatusByNameAsync("Paid", Arg.Any<CancellationToken>())
            .Returns(CourseRegistrationStatus.Reconstitute(1, "Paid"));

        var result = await service.CreateCourseRegistrationStatusAsync(new CreateCourseRegistrationStatusInput("Paid"));

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        await repo.DidNotReceive().AddAsync(Arg.Any<CourseRegistrationStatus>(), Arg.Any<CancellationToken>());
        cache.DidNotReceive().ResetEntity(Arg.Any<CourseRegistrationStatus>());
        cache.DidNotReceive().SetEntity(Arg.Any<CourseRegistrationStatus>());
    }

    [Fact]
    public async Task Update_Should_Return_400_When_Name_Empty()
    {
        var repo = Substitute.For<ICourseRegistrationStatusRepository>();
        var cache = Substitute.For<ICourseRegistrationStatusCache>();
        cache.GetByIdAsync(Arg.Any<int>(), Arg.Any<Func<CancellationToken, Task<CourseRegistrationStatus?>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<CancellationToken, Task<CourseRegistrationStatus?>>>()(ci.Arg<CancellationToken>()));
        repo.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(CourseRegistrationStatus.Reconstitute(1, "Paid"));
        var service = new CourseRegistrationStatusService(cache, repo);

        var result = await service.UpdateCourseRegistrationStatusAsync(new UpdateCourseRegistrationStatusInput(1, " "));

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        cache.DidNotReceive().ResetEntity(Arg.Any<CourseRegistrationStatus>());
        cache.DidNotReceive().SetEntity(Arg.Any<CourseRegistrationStatus>());
    }

    [Fact]
    public async Task Update_Should_Return_200_When_Valid()
    {
        var repo = Substitute.For<ICourseRegistrationStatusRepository>();
        var cache = Substitute.For<ICourseRegistrationStatusCache>();
        cache.GetByIdAsync(Arg.Any<int>(), Arg.Any<Func<CancellationToken, Task<CourseRegistrationStatus?>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<CancellationToken, Task<CourseRegistrationStatus?>>>()(ci.Arg<CancellationToken>()));
        repo.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(CourseRegistrationStatus.Reconstitute(1, "Paid"));
        repo.UpdateAsync(Arg.Any<int>(), Arg.Any<CourseRegistrationStatus>(), Arg.Any<CancellationToken>())
            .Returns(CourseRegistrationStatus.Reconstitute(1, "Paid Updated"));
        var service = new CourseRegistrationStatusService(cache, repo);

        var result = await service.UpdateCourseRegistrationStatusAsync(new UpdateCourseRegistrationStatusInput(1, "Paid"));

        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal("Paid Updated", result.Value.Name);
        cache.Received(1).ResetEntity(Arg.Is<CourseRegistrationStatus>(s => s.Id == 1));
        cache.Received(1).SetEntity(Arg.Is<CourseRegistrationStatus>(s => s.Id == 1 && s.Name == "Paid Updated"));
    }

    [Fact]
    public async Task Update_Should_Return_404_When_Status_NotFound()
    {
        var service = CreateService(out var repo, out var cache);
        repo.GetByIdAsync(99, Arg.Any<CancellationToken>())
            .Returns((CourseRegistrationStatus?)null);

        var result = await service.UpdateCourseRegistrationStatusAsync(new UpdateCourseRegistrationStatusInput(99, "Paid"));

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        cache.DidNotReceive().ResetEntity(Arg.Any<CourseRegistrationStatus>());
        cache.DidNotReceive().SetEntity(Arg.Any<CourseRegistrationStatus>());
    }

    [Fact]
    public async Task Update_Should_Return_500_When_Repository_Throws()
    {
        var service = CreateService(out var repo, out var cache);
        repo.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(CourseRegistrationStatus.Reconstitute(1, "Paid"));
        repo.UpdateAsync(Arg.Any<int>(), Arg.Any<CourseRegistrationStatus>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CourseRegistrationStatus?>(new Exception("db failure")));

        var result = await service.UpdateCourseRegistrationStatusAsync(new UpdateCourseRegistrationStatusInput(1, "Paid Updated"));

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Contains("An error occurred while updating the course registration status", result.ErrorMessage);
        cache.DidNotReceive().SetEntity(Arg.Any<CourseRegistrationStatus>());
    }

    [Fact]
    public void Constructor_Should_Throw_When_Dependencies_Are_Null()
    {
        var cache = Substitute.For<ICourseRegistrationStatusCache>();
        var repo = Substitute.For<ICourseRegistrationStatusRepository>();

        Assert.Throws<ArgumentNullException>(() => new CourseRegistrationStatusService(null!, repo));
        Assert.Throws<ArgumentNullException>(() => new CourseRegistrationStatusService(cache, null!));
    }
}

