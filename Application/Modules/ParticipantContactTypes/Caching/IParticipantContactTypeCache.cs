using Backend.Application.Common.Caching;
using Backend.Domain.Modules.ParticipantContactTypes.Models;

namespace Backend.Application.Modules.ParticipantContactTypes.Caching;

public interface IParticipantContactTypeCache : ICacheEntityBase<ParticipantContactType, int>
{
    Task<IReadOnlyList<ParticipantContactType>> GetAllAsync(Func<CancellationToken, Task<IReadOnlyList<ParticipantContactType>>> factory, CancellationToken ct);
    Task<ParticipantContactType?> GetByIdAsync(int id, Func<CancellationToken, Task<ParticipantContactType?>> factory, CancellationToken ct);
    Task<ParticipantContactType?> GetByNameAsync(string name, Func<CancellationToken, Task<ParticipantContactType?>> factory, CancellationToken ct);
}
