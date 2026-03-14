using Backend.Application.Common;
using Backend.Application.Modules.CourseEventTypes.Caching;
using Backend.Application.Modules.CourseEventTypes.Inputs;

using Backend.Domain.Modules.CourseEventTypes.Contracts;
using Backend.Domain.Modules.CourseEventTypes.Models;

namespace Backend.Application.Modules.CourseEventTypes;

public sealed class CourseEventTypeService(ICourseEventTypeCache cache, ICourseEventTypeRepository courseEventTypeRepository) : ICourseEventTypeService
{
    private readonly ICourseEventTypeCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ICourseEventTypeRepository _courseEventTypeRepository = courseEventTypeRepository ?? throw new ArgumentNullException(nameof(courseEventTypeRepository));

    public async Task<Result<CourseEventType>> CreateCourseEventTypeAsync(CreateCourseEventTypeInput courseEventType, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseEventType == null)
            {
                return Result<CourseEventType>.BadRequest("Course event type cannot be null.");
            }

            var existingCourseEventType = await _courseEventTypeRepository.GetCourseEventTypeByTypeNameAsync(courseEventType.Name, cancellationToken);

            if (existingCourseEventType is not null)
                return Result<CourseEventType>.BadRequest("A typename with the same name already exists.");

            var newCourseEventType = CourseEventType.Create(courseEventType.Name);

            var createdCourseEventType = await _courseEventTypeRepository.AddAsync(newCourseEventType, cancellationToken);
            _cache.ResetEntity(createdCourseEventType);
            _cache.SetEntity(createdCourseEventType);

            return Result<CourseEventType>.Ok(createdCourseEventType);
        }
        catch (ArgumentException ex)
        {
            return Result<CourseEventType>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<CourseEventType>.Error("An error occurred while creating the course event type.");
        }
    }

    public async Task<Result<IReadOnlyList<CourseEventType>>> GetAllCourseEventTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var courseEventTypes = await _cache.GetAllAsync(
                token => _courseEventTypeRepository.GetAllAsync(token),
                cancellationToken);

            return Result<IReadOnlyList<CourseEventType>>.Ok(courseEventTypes);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<CourseEventType>>.Error("An error occurred while retrieving course event types.");
        }
    }

    public async Task<Result<CourseEventType>> GetCourseEventTypeByIdAsync(int courseEventTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseEventTypeId <= 0)
            {
                return Result<CourseEventType>.BadRequest("Course event type ID must be greater than zero.");
            }

            var courseEventType = await _cache.GetByIdAsync(
                courseEventTypeId,
                token => _courseEventTypeRepository.GetByIdAsync(courseEventTypeId, token),
                cancellationToken);

            if (courseEventType == null)
            {
                return Result<CourseEventType>.NotFound($"Course event type with ID '{courseEventTypeId}' not found.");
            }

            return Result<CourseEventType>.Ok(courseEventType);
        }
        catch (Exception)
        {
            return Result<CourseEventType>.Error("An error occurred while retrieving the course event type.");
        }
    }

    public async Task<Result<CourseEventType>> GetCourseEventTypeByTypeNameAsync(string typeName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return Result<CourseEventType>.BadRequest("Course event type name is required.");
            }

            var courseEventType = await _courseEventTypeRepository.GetCourseEventTypeByTypeNameAsync(typeName, cancellationToken);

            if (courseEventType == null)
            {
                return Result<CourseEventType>.NotFound($"Course event type with name '{typeName}' not found.");
            }

            return Result<CourseEventType>.Ok(courseEventType);
        }
        catch (Exception)
        {
            return Result<CourseEventType>.Error("An error occurred while retrieving the course event type.");
        }
    }

    public async Task<Result<CourseEventType>> UpdateCourseEventTypeAsync(UpdateCourseEventTypeInput courseEventType, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseEventType == null)
            {
                return Result<CourseEventType>.BadRequest("Course event type cannot be null.");
            }

            var existingCourseEventType = await _courseEventTypeRepository.GetByIdAsync(courseEventType.Id, cancellationToken);

            if (existingCourseEventType == null)
            {
                return Result<CourseEventType>.NotFound($"Course event type with ID '{courseEventType.Id}' not found.");
            }

            existingCourseEventType.Update(courseEventType.Name);

            var updatedCourseEventType = await _courseEventTypeRepository.UpdateAsync(existingCourseEventType.Id, existingCourseEventType, cancellationToken);

            if (updatedCourseEventType == null)
            {
                return Result<CourseEventType>.Error("Failed to update course event type.");
            }

            _cache.ResetEntity(existingCourseEventType);
            _cache.SetEntity(updatedCourseEventType);

            return Result<CourseEventType>.Ok(updatedCourseEventType);
        }
        catch (ArgumentException ex)
        {
            return Result<CourseEventType>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<CourseEventType>.Error("An error occurred while updating the course event type.");
        }
    }

    public async Task<Result<bool>> DeleteCourseEventTypeAsync(int courseEventTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseEventTypeId <= 0)
            {
                return Result<bool>.BadRequest("Course event type ID must be greater than zero.");
            }

            var existingCourseEventType = await _courseEventTypeRepository.GetByIdAsync(courseEventTypeId, cancellationToken);
            if (existingCourseEventType == null)
            {
                return Result<bool>.NotFound($"Course event type with ID '{courseEventTypeId}' not found.");
            }

            var isInUse = await _courseEventTypeRepository.IsInUseAsync(courseEventTypeId, cancellationToken);
            if (isInUse)
            {
                return Result<bool>.Conflict($"Cannot delete course event type with ID '{courseEventTypeId}' because it is being used by one or more course events.");
            }

            var isDeleted = await _courseEventTypeRepository.RemoveAsync(courseEventTypeId, cancellationToken);
            if (!isDeleted)
            {
                return Result<bool>.Error("Failed to delete course event type.");
            }

            _cache.ResetEntity(existingCourseEventType);

            return Result<bool>.Ok(true);
        }
        catch (Exception)
        {
            return Result<bool>.Error("An error occurred while deleting the course event type.");
        }
    }
}

