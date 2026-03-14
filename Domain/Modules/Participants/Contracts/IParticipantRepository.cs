using Backend.Domain.Modules.Participants.Models;
using Backend.Domain.Common.Base;

namespace Backend.Domain.Modules.Participants.Contracts;

public interface IParticipantRepository : IRepositoryBase<Participant, Guid>
{
    Task<bool> HasRegistrationsAsync(Guid participantId, CancellationToken cancellationToken);
}

