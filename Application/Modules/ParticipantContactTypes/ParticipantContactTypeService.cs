using Backend.Application.Common;
using Backend.Application.Modules.ParticipantContactTypes.Caching;
using Backend.Application.Modules.ParticipantContactTypes.Inputs;

using Backend.Domain.Modules.ParticipantContactTypes.Contracts;
using Backend.Domain.Modules.ParticipantContactTypes.Models;

namespace Backend.Application.Modules.ParticipantContactTypes;

public sealed class ParticipantContactTypeService(IParticipantContactTypeCache cache, IParticipantContactTypeRepository repository) : IParticipantContactTypeService
{
    private readonly IParticipantContactTypeCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly IParticipantContactTypeRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<Result<ParticipantContactType>> CreateParticipantContactTypeAsync(CreateParticipantContactTypeInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
                return Result<ParticipantContactType>.BadRequest("Participant contact type cannot be null.");

            var existing = await _repository.GetByNameAsync(input.Name, cancellationToken);
            if (existing is not null)
                return Result<ParticipantContactType>.BadRequest("A participant contact type with the same name already exists.");

            var created = await _repository.AddAsync(ParticipantContactType.Create(input.Name), cancellationToken);
            _cache.ResetEntity(created);
            _cache.SetEntity(created);
            return Result<ParticipantContactType>.Ok(created);
        }
        catch (ArgumentException ex)
        {
            return Result<ParticipantContactType>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<ParticipantContactType>.Error("An error occurred while creating the participant contact type.");
        }
    }

    public async Task<Result<IReadOnlyList<ParticipantContactType>>> GetAllParticipantContactTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var participantContactTypes = await _cache.GetAllAsync(
                token => _repository.GetAllAsync(token),
                cancellationToken);
            return Result<IReadOnlyList<ParticipantContactType>>.Ok(participantContactTypes);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<ParticipantContactType>>.Error("An error occurred while retrieving participant contact types.");
        }
    }

    public async Task<Result<ParticipantContactType>> GetParticipantContactTypeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero.", nameof(id));

            var participantContactType = await _cache.GetByIdAsync(
                id,
                token => _repository.GetByIdAsync(id, token),
                cancellationToken);
            if (participantContactType == null)
                return Result<ParticipantContactType>.NotFound($"Participant contact type with ID '{id}' not found.");

            return Result<ParticipantContactType>.Ok(participantContactType);
        }
        catch (ArgumentException ex)
        {
            return Result<ParticipantContactType>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<ParticipantContactType>.Error("An error occurred while retrieving the participant contact type.");
        }
    }

    public async Task<Result<ParticipantContactType>> GetParticipantContactTypeByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", nameof(name));

            var participantContactType = await _cache.GetByNameAsync(
                name,
                token => _repository.GetByNameAsync(name, token),
                cancellationToken);
            if (participantContactType == null)
                return Result<ParticipantContactType>.NotFound($"Participant contact type with name '{name}' not found.");

            return Result<ParticipantContactType>.Ok(participantContactType);
        }
        catch (ArgumentException ex)
        {
            return Result<ParticipantContactType>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<ParticipantContactType>.Error("An error occurred while retrieving the participant contact type.");
        }
    }

    public async Task<Result<ParticipantContactType>> UpdateParticipantContactTypeAsync(UpdateParticipantContactTypeInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
                return Result<ParticipantContactType>.BadRequest("Participant contact type cannot be null.");

            var existingParticipantContactType = await _repository.GetByIdAsync(input.Id, cancellationToken);
            if (existingParticipantContactType == null)
                return Result<ParticipantContactType>.NotFound($"Participant contact type with ID '{input.Id}' not found.");

            existingParticipantContactType.Update(input.Name);
            var updatedParticipantContactType = await _repository.UpdateAsync(existingParticipantContactType.Id, existingParticipantContactType, cancellationToken);
            if (updatedParticipantContactType == null)
                return Result<ParticipantContactType>.Error("Failed to update participant contact type.");
            _cache.ResetEntity(existingParticipantContactType);
            _cache.SetEntity(updatedParticipantContactType);

            return Result<ParticipantContactType>.Ok(updatedParticipantContactType);
        }
        catch (ArgumentException ex)
        {
            return Result<ParticipantContactType>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<ParticipantContactType>.Error("An error occurred while updating the participant contact type.");
        }
    }

    public async Task<Result<bool>> DeleteParticipantContactTypeAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero.", nameof(id));

            var existingParticipantContactType = await _repository.GetByIdAsync(id, cancellationToken);
            if (existingParticipantContactType == null)
                return Result<bool>.NotFound($"Participant contact type with ID '{id}' not found.");

            if (await _repository.IsInUseAsync(id, cancellationToken))
                return Result<bool>.Conflict($"Cannot delete participant contact type with ID '{id}' because it is in use.");

            var isDeleted = await _repository.RemoveAsync(id, cancellationToken);
            if (!isDeleted)
                return Result<bool>.Error("Failed to delete participant contact type.");

            _cache.ResetEntity(existingParticipantContactType);
            return Result<bool>.Ok(true);
        }
        catch (ArgumentException ex)
        {
            return Result<bool>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<bool>.Error("An error occurred while deleting the participant contact type.");
        }
    }
}

