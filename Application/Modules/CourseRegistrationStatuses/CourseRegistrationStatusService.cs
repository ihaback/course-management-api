using Backend.Application.Common;
using Backend.Application.Modules.CourseRegistrationStatuses.Caching;
using Backend.Application.Modules.CourseRegistrationStatuses.Inputs;

using Backend.Domain.Modules.CourseRegistrationStatuses.Contracts;
using Backend.Domain.Modules.CourseRegistrationStatuses.Models;

namespace Backend.Application.Modules.CourseRegistrationStatuses;

public sealed class CourseRegistrationStatusService(ICourseRegistrationStatusCache cache, ICourseRegistrationStatusRepository repository) : ICourseRegistrationStatusService
{
    private readonly ICourseRegistrationStatusCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ICourseRegistrationStatusRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<Result<CourseRegistrationStatus>> CreateCourseRegistrationStatusAsync(CreateCourseRegistrationStatusInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
            {
                return Result<CourseRegistrationStatus>.BadRequest("Course registration status cannot be null.");
            }

            var existingCourseRegistrationStatus = await _repository.GetCourseRegistrationStatusByNameAsync(input.Name, cancellationToken);

            if (existingCourseRegistrationStatus is not null)
                return Result<CourseRegistrationStatus>.BadRequest("A status with the same name already exists.");

            var newStatus = CourseRegistrationStatus.Create(input.Name);
            var createdStatus = await _repository.AddAsync(newStatus, cancellationToken);
            _cache.ResetEntity(createdStatus);
            _cache.SetEntity(createdStatus);

            return Result<CourseRegistrationStatus>.Ok(createdStatus);
        }
        catch (ArgumentException ex)
        {
            return Result<CourseRegistrationStatus>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<CourseRegistrationStatus>.Error("An error occurred while creating the course registration status.");
        }
    }

    public async Task<Result<IReadOnlyList<CourseRegistrationStatus>>> GetAllCourseRegistrationStatusesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var statuses = await _cache.GetAllAsync(
                token => _repository.GetAllAsync(token),
                cancellationToken);

            return Result<IReadOnlyList<CourseRegistrationStatus>>.Ok(statuses);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<CourseRegistrationStatus>>.Error("An error occurred while retrieving course registration statuses.");
        }
    }

    public async Task<Result<CourseRegistrationStatus>> GetCourseRegistrationStatusByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id < 0)
                return Result<CourseRegistrationStatus>.BadRequest("Id must be zero or positive.");

            var status = await _cache.GetByIdAsync(
                id,
                token => _repository.GetByIdAsync(id, token),
                cancellationToken);
            if (status == null)
            {
                return Result<CourseRegistrationStatus>.NotFound($"Course registration status with ID '{id}' not found.");
            }

            return Result<CourseRegistrationStatus>.Ok(status);
        }
        catch (ArgumentException ex)
        {
            return Result<CourseRegistrationStatus>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<CourseRegistrationStatus>.Error("An error occurred while retrieving the course registration status.");
        }
    }

    public async Task<Result<CourseRegistrationStatus>> GetCourseRegistrationStatusByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", nameof(name));

            var status = await _cache.GetByNameAsync(
                name,
                token => _repository.GetCourseRegistrationStatusByNameAsync(name, token),
                cancellationToken);

            if (status == null)
            {
                return Result<CourseRegistrationStatus>.NotFound($"Course registration status with name '{name}' not found.");
            }

            return Result<CourseRegistrationStatus>.Ok(status);
        }
        catch (ArgumentException ex)
        {
            return Result<CourseRegistrationStatus>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<CourseRegistrationStatus>.Error("An error occurred while retrieving the course registration status.");
        }
    }

    public async Task<Result<CourseRegistrationStatus>> UpdateCourseRegistrationStatusAsync(UpdateCourseRegistrationStatusInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
            {
                return Result<CourseRegistrationStatus>.BadRequest("Course registration status cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(input.Name))
                return Result<CourseRegistrationStatus>.BadRequest("Name cannot be empty or whitespace.");

            var existingStatus = await _repository.GetByIdAsync(input.Id, cancellationToken);
            if (existingStatus == null)
            {
                return Result<CourseRegistrationStatus>.NotFound($"Course registration status with ID '{input.Id}' not found.");
            }

            _cache.ResetEntity(existingStatus);
            existingStatus.Update(input.Name);
            var updatedStatus = await _repository.UpdateAsync(existingStatus.Id, existingStatus, cancellationToken);

            if (updatedStatus == null)
            {
                return Result<CourseRegistrationStatus>.Error("Failed to update course registration status.");
            }

            _cache.SetEntity(updatedStatus);

            return Result<CourseRegistrationStatus>.Ok(updatedStatus);
        }
        catch (ArgumentException ex)
        {
            return Result<CourseRegistrationStatus>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<CourseRegistrationStatus>.Error("An error occurred while updating the course registration status.");
        }
    }

    public async Task<Result<bool>> DeleteCourseRegistrationStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id < 0)
                return Result<bool>.BadRequest("Id must be zero or positive.");

            var existingStatus = await _repository.GetByIdAsync(id, cancellationToken);

            if (existingStatus == null)
            {
                return Result<bool>.NotFound($"Course registration status with ID '{id}' not found.");
            }

            var isStatusInUse = await _repository.IsInUseAsync(id, cancellationToken);

            if (isStatusInUse)
            {
                return Result<bool>.Conflict($"Cannot delete course registration status with ID '{id}' because it is in use.");
            }

            var deleted = await _repository.RemoveAsync(id, cancellationToken);
            if (!deleted)
            {
                return Result<bool>.Error("Failed to delete course registration status.");
            }

            _cache.ResetEntity(existingStatus);

            return Result<bool>.Ok(true);
        }
        catch (ArgumentException ex)
        {
            return Result<bool>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<bool>.Error("An error occurred while deleting the course registration status.");
        }
    }
}

