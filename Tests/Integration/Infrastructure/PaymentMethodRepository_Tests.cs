using Backend.Domain.Modules.CourseRegistrations.Models;
using Backend.Domain.Modules.CourseRegistrationStatuses.Models;
using PaymentMethodModel = Backend.Domain.Modules.PaymentMethods.Models.PaymentMethod;
using Backend.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Integration.Infrastructure;

[Collection(SqliteInMemoryCollection.Name)]
public class PaymentMethodRepository_Tests(SqliteInMemoryFixture fixture)
{
    [Fact]
    public async Task CreatePaymentMethodAsync_ShouldPersist_And_BeReadableByIdAndName()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new PaymentMethodRepository(context);
        var name = $"Method-{Guid.NewGuid():N}";

        var created = await repo.AddAsync(PaymentMethodModel.Reconstitute(0, name), CancellationToken.None);
        var byId = await repo.GetByIdAsync(created.Id, CancellationToken.None);
        var byName = await repo.GetByNameAsync(name, CancellationToken.None);

        Assert.NotNull(byId);
        Assert.NotNull(byName);
        Assert.Equal(name, byId!.Name);
        Assert.Equal(created.Id, byName!.Id);

        var persisted = await context.PaymentMethods
            .AsNoTracking()
            .SingleAsync(x => x.Id == created.Id, CancellationToken.None);

        Assert.Equal(name, persisted.Name);
    }

    [Fact]
    public async Task GetAllPaymentMethodsAsync_ShouldReturnDescendingById()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new PaymentMethodRepository(context);
        var first = await repo.AddAsync(PaymentMethodModel.Reconstitute(0, $"MethodA-{Guid.NewGuid():N}"), CancellationToken.None);
        var second = await repo.AddAsync(PaymentMethodModel.Reconstitute(0, $"MethodB-{Guid.NewGuid():N}"), CancellationToken.None);

        var all = await repo.GetAllAsync(CancellationToken.None);
        var firstIndex = all.ToList().FindIndex(x => x.Id == first.Id);
        var secondIndex = all.ToList().FindIndex(x => x.Id == second.Id);

        Assert.True(firstIndex >= 0);
        Assert.True(secondIndex >= 0);
        Assert.True(secondIndex < firstIndex);
    }

    [Fact]
    public async Task UpdatePaymentMethodAsync_ShouldPersistChanges()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new PaymentMethodRepository(context);
        var created = await repo.AddAsync(PaymentMethodModel.Reconstitute(0, $"Method-{Guid.NewGuid():N}"), CancellationToken.None);

        var updated = await repo.UpdateAsync(created.Id, PaymentMethodModel.Reconstitute(created.Id, "Updated"), CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Updated", updated!.Name);

        var persisted = await context.PaymentMethods.AsNoTracking().SingleAsync(x => x.Id == created.Id, CancellationToken.None);
        Assert.Equal("Updated", persisted.Name);
    }

    [Fact]
    public async Task IsInUseAsync_ShouldReturnTrue_WhenReferencedByCourseRegistration()
    {
        await using var context = fixture.CreateDbContext();
        var paymentMethodRepo = new PaymentMethodRepository(context);
        var paymentMethod = await paymentMethodRepo.AddAsync(
            PaymentMethodModel.Reconstitute(0, $"Method-{Guid.NewGuid():N}"),
            CancellationToken.None);
        var participant = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        var registrationRepo = new CourseRegistrationRepository(context);

        await registrationRepo.AddAsync(
            CourseRegistration.Reconstitute(
                Guid.NewGuid(),
                participant.Id,
                courseEvent.Id,
                DateTime.UtcNow,
                CourseRegistrationStatus.Pending,
                PaymentMethodModel.Reconstitute(paymentMethod.Id, paymentMethod.Name)),
            CancellationToken.None);

        var inUse = await paymentMethodRepo.IsInUseAsync(paymentMethod.Id, CancellationToken.None);

        Assert.True(inUse);
    }

    [Fact]
    public async Task DeletePaymentMethodAsync_ShouldRemovePaymentMethod()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new PaymentMethodRepository(context);
        var created = await repo.AddAsync(PaymentMethodModel.Reconstitute(0, $"Method-{Guid.NewGuid():N}"), CancellationToken.None);

        var removed = await repo.RemoveAsync(created.Id, CancellationToken.None);
        var byId = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.True(removed);
        Assert.Null(byId);

        var exists = await context.PaymentMethods.AnyAsync(x => x.Id == created.Id, CancellationToken.None);
        Assert.False(exists);
    }
}

