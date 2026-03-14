using Backend.Application.Common;
using Backend.Application.Modules.InstructorRoles.Caching;
using Backend.Application.Modules.InstructorRoles.Inputs;

using Backend.Domain.Modules.InstructorRoles.Contracts;
using Backend.Domain.Modules.InstructorRoles.Models;

namespace Backend.Application.Modules.InstructorRoles;

public class InstructorRoleService(IInstructorRoleCache cache, IInstructorRoleRepository repository) : IInstructorRoleService
{
    private readonly IInstructorRoleCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly IInstructorRoleRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<Result<InstructorRole>> CreateInstructorRoleAsync(CreateInstructorRoleInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
            {
                return Result<InstructorRole>.BadRequest("Role cannot be null.");
            }

            var role = InstructorRole.Create(input.Name);
            var created = await _repository.AddAsync(role, cancellationToken);
            _cache.ResetEntity(created);
            _cache.SetEntity(created);

            return Result<InstructorRole>.Ok(created);
        }
        catch (ArgumentException ex)
        {
            return Result<InstructorRole>.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<InstructorRole>.Error($"An error occurred while creating the instructor role: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<InstructorRole>>> GetAllInstructorRolesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _cache.GetAllAsync(
                token => _repository.GetAllAsync(token),
                cancellationToken);
            return Result<IReadOnlyList<InstructorRole>>.Ok(roles);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<InstructorRole>>.Error($"An error occurred while retrieving instructor roles: {ex.Message}");
        }
    }

    public async Task<Result<InstructorRole>> GetInstructorRoleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id < 1)
            {
                return Result<InstructorRole>.BadRequest("Id must be greater than zero.");
            }

            var role = await _cache.GetByIdAsync(
                id,
                token => _repository.GetByIdAsync(id, token),
                cancellationToken);
            if (role == null)
            {
                return Result<InstructorRole>.NotFound($"Instructor role with ID '{id}' not found.");
            }

            return Result<InstructorRole>.Ok(role);
        }
        catch (Exception ex)
        {
            return Result<InstructorRole>.Error($"An error occurred while retrieving the instructor role: {ex.Message}");
        }
    }

    public async Task<Result<InstructorRole>> UpdateInstructorRoleAsync(UpdateInstructorRoleInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
            {
                return Result<InstructorRole>.BadRequest("Role cannot be null.");
            }

            if (input.Id < 1)
            {
                return Result<InstructorRole>.BadRequest("Id must be greater than zero.");
            }

            var existingRole = await _repository.GetByIdAsync(input.Id, cancellationToken);
            if (existingRole == null)
            {
                return Result<InstructorRole>.NotFound($"Instructor role with ID '{input.Id}' not found.");
            }

            existingRole.Update(input.Name);
            var updatedInstructorRole = await _repository.UpdateAsync(existingRole.Id, existingRole, cancellationToken);
            if (updatedInstructorRole == null)
            {
                return Result<InstructorRole>.NotFound($"Instructor role with ID '{input.Id}' not found.");
            }

            _cache.ResetEntity(existingRole);
            _cache.SetEntity(updatedInstructorRole);

            return Result<InstructorRole>.Ok(updatedInstructorRole);
        }
        catch (ArgumentException ex)
        {
            return Result<InstructorRole>.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<InstructorRole>.Error($"An error occurred while updating the instructor role: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteInstructorRoleAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id < 1)
            {
                return Result<bool>.BadRequest("Id must be greater than zero.");
            }

            var existingRole = await _repository.GetByIdAsync(id, cancellationToken);
            if (existingRole == null)
            {
                return Result<bool>.NotFound($"Instructor role with ID '{id}' not found.");
            }

            var isDeleted = await _repository.RemoveAsync(id, cancellationToken);
            if (!isDeleted)
            {
                return Result<bool>.Error("Failed to delete instructor role.");
            }

            _cache.ResetEntity(existingRole);

            return Result<bool>.Ok(true);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            return Result<bool>.Conflict($"Cannot delete instructor role with ID '{id}' because it is in use.");
        }
        catch (Exception ex)
        {
            return Result<bool>.Error($"An error occurred while deleting the instructor role: {ex.Message}");
        }
    }
}

