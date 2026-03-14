using Backend.Application.Common;
using Backend.Application.Modules.Participants.Inputs;
using Backend.Application.Modules.Participants.Outputs;
using Backend.Domain.Modules.ParticipantContactTypes.Contracts;
using Backend.Domain.Modules.ParticipantContactTypes.Models;
using Backend.Domain.Modules.Participants.Contracts;
using Backend.Domain.Modules.Participants.Models;

namespace Backend.Application.Modules.Participants;

public sealed class ParticipantService(
    IParticipantRepository participantRepository,
    IParticipantContactTypeRepository participantContactTypeRepository) : IParticipantService
{
    private readonly IParticipantRepository _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
    private readonly IParticipantContactTypeRepository _participantContactTypeRepository = participantContactTypeRepository ?? throw new ArgumentNullException(nameof(participantContactTypeRepository));

    public async Task<Result<Participant>> CreateParticipantAsync(CreateParticipantInput participant, CancellationToken cancellationToken = default)
    {
        try
        {
            if (participant == null)
            {
                return Result<Participant>.BadRequest("Participant cannot be null.");
            }

            var contactType = await _participantContactTypeRepository.GetByIdAsync(participant.ContactTypeId, cancellationToken);
            if (contactType == null)
            {
                return Result<Participant>.NotFound($"Participant contact type with ID '{participant.ContactTypeId}' not found.");
            }

            var newParticipant = Participant.Create(
                participant.FirstName,
                participant.LastName,
                participant.Email,
                participant.PhoneNumber,
                contactType
            );

            var createdParticipant = await _participantRepository.AddAsync(newParticipant, cancellationToken);

            return Result<Participant>.Ok(createdParticipant);
        }
        catch (ArgumentException ex)
        {
            return Result<Participant>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<Participant>.Error("An error occurred while creating the participant.");
        }
    }

    public async Task<Result<IReadOnlyList<Participant>>> GetAllParticipantsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var participants = await _participantRepository.GetAllAsync(cancellationToken);

            return Result<IReadOnlyList<Participant>>.Ok(participants);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<Participant>>.Error("An error occurred while retrieving participants.");
        }
    }

    public async Task<Result<ParticipantDetails>> GetParticipantByIdAsync(Guid participantId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (participantId == Guid.Empty)
            {
                return Result<ParticipantDetails>.BadRequest("Participant ID cannot be empty.");
            }

            var existingParticipant = await _participantRepository.GetByIdAsync(participantId, cancellationToken);

            if (existingParticipant == null)
            {
                return Result<ParticipantDetails>.NotFound($"Participant with ID '{participantId}' not found.");
            }

            var details = new ParticipantDetails(
                existingParticipant.Id,
                existingParticipant.FirstName,
                existingParticipant.LastName,
                existingParticipant.Email.Value,
                existingParticipant.PhoneNumber.Value,
                new ParticipantLookupItem(
                    existingParticipant.ContactType.Id,
                    existingParticipant.ContactType.Name)
            );

            return Result<ParticipantDetails>.Ok(details);
        }
        catch (Exception)
        {
            return Result<ParticipantDetails>.Error("An error occurred while retrieving the participant.");
        }
    }

    public async Task<Result<Participant>> UpdateParticipantAsync(UpdateParticipantInput participant, CancellationToken cancellationToken = default)
    {
        try
        {
            if (participant == null)
            {
                return Result<Participant>.BadRequest("Participant cannot be null.");
            }

            if (participant.Id == Guid.Empty)
            {
                return Result<Participant>.BadRequest("Participant ID cannot be empty.");
            }

            var existingParticipant = await _participantRepository.GetByIdAsync(participant.Id, cancellationToken);
            if (existingParticipant == null)
            {
                return Result<Participant>.NotFound($"Participant with ID '{participant.Id}' not found.");
            }

            var contactType = await _participantContactTypeRepository.GetByIdAsync(participant.ContactTypeId, cancellationToken);
            if (contactType == null)
            {
                return Result<Participant>.NotFound($"Participant contact type with ID '{participant.ContactTypeId}' not found.");
            }

            existingParticipant.Update(
                participant.FirstName,
                participant.LastName,
                participant.Email,
                participant.PhoneNumber,
                contactType
            );

            var updatedParticipant = await _participantRepository.UpdateAsync(existingParticipant.Id, existingParticipant, cancellationToken);

            if (updatedParticipant == null)
            {
                return Result<Participant>.Error("Failed to update participant.");
            }

            return Result<Participant>.Ok(updatedParticipant);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("modified by another user"))
        {
            return Result<Participant>.Conflict("The participant was modified by another user. Please refresh and try again.");
        }
        catch (ArgumentException ex)
        {
            return Result<Participant>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<Participant>.Error("An error occurred while updating the participant.");
        }
    }

    public async Task<Result<bool>> DeleteParticipantAsync(Guid participantId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (participantId == Guid.Empty)
            {
                return Result<bool>.BadRequest("Participant ID cannot be empty.");
            }

            var existingParticipant = await _participantRepository.GetByIdAsync(participantId, cancellationToken);
            if (existingParticipant == null)
            {
                return Result<bool>.NotFound($"Participant with ID '{participantId}' not found.");
            }

            var hasRegistrations = await _participantRepository.HasRegistrationsAsync(participantId, cancellationToken);
            if (hasRegistrations)
            {
                return Result<bool>.Conflict("Cannot delete participant because they have course registrations.");
            }

            var isDeleted = await _participantRepository.RemoveAsync(participantId, cancellationToken);

            if (!isDeleted)
            {
                return Result<bool>.Error("Failed to delete participant.");
            }

            return Result<bool>.Ok(true);
        }
        catch (KeyNotFoundException ex)
        {
            return Result<bool>.NotFound(ex.Message);
        }
        catch (Exception)
        {
            return Result<bool>.Error("An error occurred while deleting the participant.");
        }
    }

}

