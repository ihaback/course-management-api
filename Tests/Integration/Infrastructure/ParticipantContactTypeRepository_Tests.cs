using Backend.Domain.Modules.ParticipantContactTypes.Models;
using Backend.Domain.Modules.Participants.Models;
using Backend.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Integration.Infrastructure;

[Collection(SqliteInMemoryCollection.Name)]
public class ParticipantContactTypeRepository_Tests(SqliteInMemoryFixture fixture)
{
    [Fact]
    public async Task CreateParticipantContactTypeAsync_ShouldPersist_And_BeReadableByIdAndName()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new ParticipantContactTypeRepository(context);
        var name = $"Contact-{Guid.NewGuid():N}";

        var created = await repo.AddAsync(ParticipantContactType.Reconstitute(1, name), CancellationToken.None);
        var byId = await repo.GetByIdAsync(created.Id, CancellationToken.None);
        var byName = await repo.GetByNameAsync(name, CancellationToken.None);

        Assert.NotNull(byId);
        Assert.NotNull(byName);
        Assert.Equal(name, byId!.Name);
        Assert.Equal(created.Id, byName!.Id);

        var persisted = await context.ParticipantContactTypes
            .AsNoTracking()
            .SingleAsync(x => x.Id == created.Id, CancellationToken.None);

        Assert.Equal(name, persisted.Name);
    }

    [Fact]
    public async Task GetAllParticipantContactTypesAsync_ShouldReturnDescendingById()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new ParticipantContactTypeRepository(context);
        var first = await repo.AddAsync(ParticipantContactType.Reconstitute(1, $"ContactA-{Guid.NewGuid():N}"), CancellationToken.None);
        var second = await repo.AddAsync(ParticipantContactType.Reconstitute(1, $"ContactB-{Guid.NewGuid():N}"), CancellationToken.None);

        var all = await repo.GetAllAsync(CancellationToken.None);
        var firstIndex = all.ToList().FindIndex(x => x.Id == first.Id);
        var secondIndex = all.ToList().FindIndex(x => x.Id == second.Id);

        Assert.True(firstIndex >= 0);
        Assert.True(secondIndex >= 0);
        Assert.True(secondIndex < firstIndex);
    }

    [Fact]
    public async Task UpdateParticipantContactTypeAsync_ShouldPersistChanges()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new ParticipantContactTypeRepository(context);
        var created = await repo.AddAsync(ParticipantContactType.Reconstitute(1, $"Contact-{Guid.NewGuid():N}"), CancellationToken.None);

        var updated = await repo.UpdateAsync(created.Id, ParticipantContactType.Reconstitute(created.Id, "Updated"), CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Updated", updated!.Name);

        var persisted = await context.ParticipantContactTypes.AsNoTracking().SingleAsync(x => x.Id == created.Id, CancellationToken.None);
        Assert.Equal("Updated", persisted.Name);
    }

    [Fact]
    public async Task IsInUseAsync_ShouldReturnTrue_WhenReferencedByParticipant()
    {
        await using var context = fixture.CreateDbContext();
        var contactTypeRepo = new ParticipantContactTypeRepository(context);
        var contactType = await contactTypeRepo.AddAsync(
            ParticipantContactType.Reconstitute(1, $"Contact-{Guid.NewGuid():N}"),
            CancellationToken.None);
        var participantRepo = new ParticipantRepository(context);

        await participantRepo.AddAsync(
            Participant.Reconstitute(
                Guid.NewGuid(),
                "Test",
                "User",
                $"user-{Guid.NewGuid():N}@example.com",
                "12345",
                ParticipantContactType.Reconstitute(contactType.Id, contactType.Name)),
            CancellationToken.None);

        var inUse = await contactTypeRepo.IsInUseAsync(contactType.Id, CancellationToken.None);

        Assert.True(inUse);
    }

    [Fact]
    public async Task DeleteParticipantContactTypeAsync_ShouldRemoveContactType()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new ParticipantContactTypeRepository(context);
        var created = await repo.AddAsync(ParticipantContactType.Reconstitute(1, $"Contact-{Guid.NewGuid():N}"), CancellationToken.None);

        var removed = await repo.RemoveAsync(created.Id, CancellationToken.None);
        var byId = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.True(removed);
        Assert.Null(byId);

        var exists = await context.ParticipantContactTypes.AnyAsync(x => x.Id == created.Id, CancellationToken.None);
        Assert.False(exists);
    }
}

