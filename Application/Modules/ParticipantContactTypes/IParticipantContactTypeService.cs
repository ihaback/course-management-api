using Backend.Application.Common;
using Backend.Application.Modules.ParticipantContactTypes.Inputs;
using Backend.Domain.Modules.ParticipantContactTypes.Models;

namespace Backend.Application.Modules.ParticipantContactTypes;

public interface IParticipantContactTypeService
{
    Task<Result<IReadOnlyList<ParticipantContactType>>> GetAllParticipantContactTypesAsync(CancellationToken cancellationToken = default);
    Task<Result<ParticipantContactType>> GetParticipantContactTypeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<ParticipantContactType>> GetParticipantContactTypeByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Result<ParticipantContactType>> CreateParticipantContactTypeAsync(CreateParticipantContactTypeInput input, CancellationToken cancellationToken = default);
    Task<Result<ParticipantContactType>> UpdateParticipantContactTypeAsync(UpdateParticipantContactTypeInput input, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteParticipantContactTypeAsync(int id, CancellationToken cancellationToken = default);
}
