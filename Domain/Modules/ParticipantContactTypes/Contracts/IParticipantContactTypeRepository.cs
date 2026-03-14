using Backend.Domain.Common.Base;
using Backend.Domain.Modules.ParticipantContactTypes.Models;

namespace Backend.Domain.Modules.ParticipantContactTypes.Contracts;

public interface IParticipantContactTypeRepository : IRepositoryBase<ParticipantContactType, int>
{
    Task<ParticipantContactType?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<bool> IsInUseAsync(int participantContactTypeId, CancellationToken cancellationToken);
}
