using Backend.Application.Common;
using Backend.Application.Modules.Participants.Inputs;
using Backend.Application.Modules.Participants.Outputs;
using Backend.Domain.Modules.Participants.Models;

namespace Backend.Application.Modules.Participants;

public interface IParticipantService
{
    Task<Result<Participant>> CreateParticipantAsync(CreateParticipantInput participant, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<Participant>>> GetAllParticipantsAsync(CancellationToken cancellationToken = default);
    Task<Result<ParticipantDetails>> GetParticipantByIdAsync(Guid participantId, CancellationToken cancellationToken = default);
    Task<Result<Participant>> UpdateParticipantAsync(UpdateParticipantInput participant, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteParticipantAsync(Guid participantId, CancellationToken cancellationToken = default);
}
