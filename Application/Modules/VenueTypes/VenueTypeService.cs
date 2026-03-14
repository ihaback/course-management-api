using Backend.Application.Common;
using Backend.Application.Modules.VenueTypes.Caching;
using Backend.Application.Modules.VenueTypes.Inputs;

using Backend.Domain.Modules.VenueTypes.Contracts;
using Backend.Domain.Modules.VenueTypes.Models;

namespace Backend.Application.Modules.VenueTypes;

public sealed class VenueTypeService(IVenueTypeCache cache, IVenueTypeRepository repository) : IVenueTypeService
{
    private readonly IVenueTypeCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly IVenueTypeRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<Result<VenueType>> CreateVenueTypeAsync(CreateVenueTypeInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
                return Result<VenueType>.BadRequest("Venue type cannot be null.");

            var existing = await _repository.GetByNameAsync(input.Name, cancellationToken);
            if (existing is not null)
                return Result<VenueType>.BadRequest("A venue type with the same name already exists.");

            var created = await _repository.AddAsync(VenueType.Create(input.Name), cancellationToken);
            _cache.ResetEntity(created);
            _cache.SetEntity(created);
            return Result<VenueType>.Ok(created);
        }
        catch (ArgumentException ex)
        {
            return Result<VenueType>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<VenueType>.Error("An error occurred while creating the venue type.");
        }
    }

    public async Task<Result<IReadOnlyList<VenueType>>> GetAllVenueTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var venueTypes = await _cache.GetAllAsync(
                token => _repository.GetAllAsync(token),
                cancellationToken);
            return Result<IReadOnlyList<VenueType>>.Ok(venueTypes);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<VenueType>>.Error("An error occurred while retrieving venue types.");
        }
    }

    public async Task<Result<VenueType>> GetVenueTypeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
                return Result<VenueType>.BadRequest("Id must be greater than zero.");

            var venueType = await _cache.GetByIdAsync(
                id,
                token => _repository.GetByIdAsync(id, token),
                cancellationToken);
            if (venueType == null)
                return Result<VenueType>.NotFound($"Venue type with ID '{id}' not found.");

            return Result<VenueType>.Ok(venueType);
        }
        catch (ArgumentException ex)
        {
            return Result<VenueType>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<VenueType>.Error("An error occurred while retrieving the venue type.");
        }
    }

    public async Task<Result<VenueType>> GetVenueTypeByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result<VenueType>.BadRequest("Name is required.");

            var venueType = await _cache.GetByNameAsync(
                name,
                token => _repository.GetByNameAsync(name, token),
                cancellationToken);
            if (venueType == null)
                return Result<VenueType>.NotFound($"Venue type with name '{name}' not found.");

            return Result<VenueType>.Ok(venueType);
        }
        catch (ArgumentException ex)
        {
            return Result<VenueType>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<VenueType>.Error("An error occurred while retrieving the venue type.");
        }
    }

    public async Task<Result<VenueType>> UpdateVenueTypeAsync(UpdateVenueTypeInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
                return Result<VenueType>.BadRequest("Venue type cannot be null.");

            var existingVenueType = await _repository.GetByIdAsync(input.Id, cancellationToken);
            if (existingVenueType == null)
                return Result<VenueType>.NotFound($"Venue type with ID '{input.Id}' not found.");

            existingVenueType.Update(input.Name);
            var updatedVenueType = await _repository.UpdateAsync(existingVenueType.Id, existingVenueType, cancellationToken);
            if (updatedVenueType == null)
                return Result<VenueType>.Error("Failed to update venue type.");
            _cache.ResetEntity(existingVenueType);
            _cache.SetEntity(updatedVenueType);

            return Result<VenueType>.Ok(updatedVenueType);
        }
        catch (ArgumentException ex)
        {
            return Result<VenueType>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<VenueType>.Error("An error occurred while updating the venue type.");
        }
    }

    public async Task<Result<bool>> DeleteVenueTypeAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
                return Result<bool>.BadRequest("Id must be greater than zero.");

            var existingVenueType = await _repository.GetByIdAsync(id, cancellationToken);
            if (existingVenueType == null)
                return Result<bool>.NotFound($"Venue type with ID '{id}' not found.");

            if (await _repository.IsInUseAsync(id, cancellationToken))
                return Result<bool>.Conflict($"Cannot delete venue type with ID '{id}' because it is in use.");

            var isDeleted = await _repository.RemoveAsync(id, cancellationToken);
            if (!isDeleted)
                return Result<bool>.Error("Failed to delete venue type.");

            _cache.ResetEntity(existingVenueType);
            return Result<bool>.Ok(true);
        }
        catch (ArgumentException ex)
        {
            return Result<bool>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<bool>.Error("An error occurred while deleting the venue type.");
        }
    }
}

