using Backend.Application.Common;
using Backend.Application.Modules.CourseEvents.Inputs;
using Backend.Application.Modules.CourseEvents.Outputs;
using Backend.Domain.Modules.CourseEvents.Contracts;
using Backend.Domain.Modules.CourseEvents.Models;
using Backend.Domain.Modules.CourseEventTypes.Contracts;
using Backend.Domain.Modules.Courses.Contracts;
using Backend.Domain.Modules.VenueTypes.Contracts;

namespace Backend.Application.Modules.CourseEvents;

public class CourseEventService(
    ICourseEventRepository courseEventRepository,
    ICourseRepository courseRepository,
    ICourseEventTypeRepository courseEventTypeRepository,
    IVenueTypeRepository venueTypeRepository) : ICourseEventService
{
    private readonly ICourseEventRepository _courseEventRepository = courseEventRepository ?? throw new ArgumentNullException(nameof(courseEventRepository));
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
    private readonly ICourseEventTypeRepository _courseEventTypeRepository = courseEventTypeRepository ?? throw new ArgumentNullException(nameof(courseEventTypeRepository));
    private readonly IVenueTypeRepository _venueTypeRepository = venueTypeRepository ?? throw new ArgumentNullException(nameof(venueTypeRepository));

    public async Task<Result<CourseEvent>> CreateCourseEventAsync(CreateCourseEventInput courseEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseEvent == null)
            {
                return Result<CourseEvent>.BadRequest("Course event cannot be null.");
            }

            var existingCourse = await _courseRepository.GetByIdAsync(courseEvent.CourseId, cancellationToken);
            if (existingCourse == null)
            {
                return Result<CourseEvent>.NotFound($"Course with ID '{courseEvent.CourseId}' not found.");
            }

            var existingCourseEventType = await _courseEventTypeRepository.GetByIdAsync(courseEvent.CourseEventTypeId, cancellationToken);
            if (existingCourseEventType == null)
            {
                return Result<CourseEvent>.NotFound($"Course event type with ID '{courseEvent.CourseEventTypeId}' not found.");
            }

            var venueType = await _venueTypeRepository.GetByIdAsync(courseEvent.VenueTypeId, cancellationToken);
            if (venueType == null)
            {
                return Result<CourseEvent>.NotFound($"Venue type with ID '{courseEvent.VenueTypeId}' not found.");
            }

            var newCourseEvent = CourseEvent.Create(
                courseEvent.CourseId,
                courseEvent.EventDate,
                courseEvent.Price,
                courseEvent.Seats,
                courseEvent.CourseEventTypeId,
                venueType);

            var createdCourseEvent = await _courseEventRepository.AddAsync(newCourseEvent, cancellationToken);

            return Result<CourseEvent>.Ok(createdCourseEvent);
        }
        catch (ArgumentException ex)
        {
            return Result<CourseEvent>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<CourseEvent>.Error("An error occurred while creating the course event.");
        }
    }

    public async Task<Result<IReadOnlyList<CourseEvent>>> GetAllCourseEventsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var courseEvents = await _courseEventRepository.GetAllAsync(cancellationToken);

            return Result<IReadOnlyList<CourseEvent>>.Ok(courseEvents);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<CourseEvent>>.Error("An error occurred while retrieving course events.");
        }
    }

    public async Task<Result<CourseEventDetails>> GetCourseEventByIdAsync(Guid courseEventId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseEventId == Guid.Empty)
            {
                return Result<CourseEventDetails>.BadRequest("Course event ID cannot be empty.");
            }

            var courseEvent = await _courseEventRepository.GetByIdAsync(courseEventId, cancellationToken);
            if (courseEvent == null)
            {
                return Result<CourseEventDetails>.NotFound($"Course event with ID '{courseEventId}' not found.");
            }

            var details = new CourseEventDetails(
                courseEvent.Id,
                courseEvent.CourseId,
                courseEvent.EventDate,
                courseEvent.Price.Value,
                courseEvent.Seats,
                new CourseEventLookupItem(courseEvent.CourseEventType.Id, courseEvent.CourseEventType.Name),
                new CourseEventLookupItem(courseEvent.VenueTypeId, courseEvent.VenueType.Name)
            );

            return Result<CourseEventDetails>.Ok(details);
        }
        catch (Exception)
        {
            return Result<CourseEventDetails>.Error("An error occurred while retrieving the course event.");
        }
    }

    public async Task<Result<IReadOnlyList<CourseEvent>>> GetCourseEventsByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseId == Guid.Empty)
            {
                return Result<IReadOnlyList<CourseEvent>>.BadRequest("Course ID cannot be empty.");
            }

            var courseEvents = await _courseEventRepository.GetCourseEventsByCourseIdAsync(courseId, cancellationToken);

            return Result<IReadOnlyList<CourseEvent>>.Ok(courseEvents);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<CourseEvent>>.Error("An error occurred while retrieving course events by course ID.");
        }
    }

    public async Task<Result<CourseEvent>> UpdateCourseEventAsync(UpdateCourseEventInput courseEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseEvent == null)
            {
                return Result<CourseEvent>.BadRequest("Course event cannot be null.");
            }

            var existingCourseEvent = await _courseEventRepository.GetByIdAsync(courseEvent.Id, cancellationToken);
            if (existingCourseEvent == null)
            {
                return Result<CourseEvent>.NotFound($"Course event with ID '{courseEvent.Id}' not found.");
            }

            var existingCourse = await _courseRepository.GetByIdAsync(courseEvent.CourseId, cancellationToken);
            if (existingCourse == null)
            {
                return Result<CourseEvent>.NotFound($"Course with ID '{courseEvent.CourseId}' not found.");
            }

            var existingCourseEventType = await _courseEventTypeRepository.GetByIdAsync(courseEvent.CourseEventTypeId, cancellationToken);
            if (existingCourseEventType == null)
            {
                return Result<CourseEvent>.NotFound($"Course event type with ID '{courseEvent.CourseEventTypeId}' not found.");
            }

            var venueType = await _venueTypeRepository.GetByIdAsync(courseEvent.VenueTypeId, cancellationToken);
            if (venueType == null)
            {
                return Result<CourseEvent>.NotFound($"Venue type with ID '{courseEvent.VenueTypeId}' not found.");
            }

            existingCourseEvent.Update(
                courseEvent.CourseId,
                courseEvent.EventDate,
                courseEvent.Price,
                courseEvent.Seats,
                courseEvent.CourseEventTypeId,
                venueType,
                existingCourseEventType);

            var updatedCourseEvent = await _courseEventRepository.UpdateAsync(existingCourseEvent.Id, existingCourseEvent, cancellationToken);
            if (updatedCourseEvent == null)
            {
                return Result<CourseEvent>.Error("Failed to update course event.");
            }

            return Result<CourseEvent>.Ok(updatedCourseEvent);
        }
        catch (KeyNotFoundException ex)
        {
            return Result<CourseEvent>.NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<CourseEvent>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<CourseEvent>.Error("An error occurred while updating the course event.");
        }
    }

    public async Task<Result<bool>> DeleteCourseEventAsync(Guid courseEventId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseEventId == Guid.Empty)
            {
                return Result<bool>.BadRequest("Course event ID cannot be empty.");
            }

            var existingCourseEvent = await _courseEventRepository.GetByIdAsync(courseEventId, cancellationToken);
            if (existingCourseEvent == null)
            {
                return Result<bool>.NotFound($"Course event with ID '{courseEventId}' not found.");
            }

            var hasRegistrations = await _courseEventRepository.HasRegistrationsAsync(courseEventId, cancellationToken);
            if (hasRegistrations)
            {
                return Result<bool>.Conflict($"Cannot delete course event with ID '{courseEventId}' because it has registrations.");
            }

            var isDeleted = await _courseEventRepository.RemoveAsync(courseEventId, cancellationToken);
            if (!isDeleted)
                return Result<bool>.NotFound($"Course event with ID '{courseEventId}' was not found.");
            return Result<bool>.Ok(true);
        }
        catch (KeyNotFoundException ex)
        {
            return Result<bool>.NotFound(ex.Message);
        }
        catch (Exception)
        {
            return Result<bool>.Error("An error occurred while deleting the course event.");
        }
    }

}

