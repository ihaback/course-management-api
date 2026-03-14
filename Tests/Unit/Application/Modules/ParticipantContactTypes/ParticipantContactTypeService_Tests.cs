using Backend.Application.Common;
using Backend.Application.Modules.ParticipantContactTypes;
using Backend.Application.Modules.ParticipantContactTypes.Caching;
using Backend.Application.Modules.ParticipantContactTypes.Inputs;
using Backend.Domain.Modules.ParticipantContactTypes.Contracts;
using Backend.Domain.Modules.ParticipantContactTypes.Models;
using NSubstitute;

namespace Backend.Tests.Unit.Application.Modules.ParticipantContactTypes;

public class ParticipantContactTypeService_Tests
{
    [Fact]
    public async Task GetAll_Should_Return_Items()
    {
        var repo = Substitute.For<IParticipantContactTypeRepository>();
        var cache = Substitute.For<IParticipantContactTypeCache>();
        cache.GetAllAsync(Arg.Any<Func<CancellationToken, Task<IReadOnlyList<ParticipantContactType>>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<CancellationToken, Task<IReadOnlyList<ParticipantContactType>>>>()(ci.Arg<CancellationToken>()));
        repo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([ParticipantContactType.Reconstitute(1, "Primary"), ParticipantContactType.Reconstitute(2, "Billing")]);
        var service = new ParticipantContactTypeService(cache, repo);

        var result = await service.GetAllParticipantContactTypesAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task GetById_Should_Use_Cache_Without_Repo_Call()
    {
        var repo = Substitute.For<IParticipantContactTypeRepository>();
        var cache = Substitute.For<IParticipantContactTypeCache>();
        var cached = ParticipantContactType.Reconstitute(7, "Billing");
        cache.GetByIdAsync(7, Arg.Any<Func<CancellationToken, Task<ParticipantContactType?>>>(), Arg.Any<CancellationToken>())
            .Returns(cached);
        var service = new ParticipantContactTypeService(cache, repo);

        var result = await service.GetParticipantContactTypeByIdAsync(7, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(cached, result.Value);
        await repo.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_Should_Reset_And_Set_Cache()
    {
        var repo = Substitute.For<IParticipantContactTypeRepository>();
        var cache = Substitute.For<IParticipantContactTypeCache>();
        var existing = ParticipantContactType.Reconstitute(3, "Primary");
        var updated = ParticipantContactType.Reconstitute(3, "Billing");

        repo.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>()).Returns(existing);
        repo.UpdateAsync(existing.Id, Arg.Any<ParticipantContactType>(), Arg.Any<CancellationToken>())
            .Returns(updated);

        var service = new ParticipantContactTypeService(cache, repo);

        var result = await service.UpdateParticipantContactTypeAsync(new UpdateParticipantContactTypeInput(existing.Id, "Billing"), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(updated, result.Value);
        cache.Received(1).ResetEntity(existing);
        cache.Received(1).SetEntity(updated);
    }
}

