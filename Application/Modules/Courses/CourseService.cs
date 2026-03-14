using Backend.Application.Common;
using Backend.Application.Modules.Courses.Inputs;

using Backend.Domain.Modules.Courses.Contracts;
using Backend.Domain.Modules.Courses.Models;

namespace Backend.Application.Modules.Courses;

public sealed class CourseService(ICourseRepository courseRepository) : ICourseService
{
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));

    public async Task<Result<Course>> CreateCourseAsync(CreateCourseInput course, CancellationToken cancellationToken = default)
    {
        try
        {
            if (course == null)
            {
                return Result<Course>.BadRequest("Course cannot be null.");
            }

            var newCourse = Course.Create(
                course.Title,
                course.Description,
                course.DurationInDays
            );

            _ = await _courseRepository.AddAsync(newCourse, cancellationToken);

            return Result<Course>.Ok(newCourse);
        }
        catch (ArgumentException ex)
        {
            return Result<Course>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<Course>.Error("An error occurred while creating the course.");
        }
    }

    public async Task<Result<IReadOnlyList<Course>>> GetAllCoursesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var courses = await _courseRepository.GetAllAsync(cancellationToken);

            return Result<IReadOnlyList<Course>>.Ok(courses);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<Course>>.Error("An error occurred while retrieving courses.");
        }
    }

    public async Task<Result<CourseWithEvents>> GetCourseByIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseId == Guid.Empty)
            {
                return Result<CourseWithEvents>.BadRequest("Course ID cannot be empty.");
            }

            var courseWithEvents = await _courseRepository.GetByIdWithEventsAsync(courseId, cancellationToken);

            if (courseWithEvents == null)
            {
                return Result<CourseWithEvents>.NotFound($"Course with ID '{courseId}' not found.");
            }

            return Result<CourseWithEvents>.Ok(courseWithEvents);
        }
        catch (Exception)
        {
            return Result<CourseWithEvents>.Error("An error occurred while retrieving the course.");
        }
    }

    public async Task<Result<Course>> UpdateCourseAsync(UpdateCourseInput course, CancellationToken cancellationToken = default)
    {
        try
        {
            if (course == null)
            {
                return Result<Course>.BadRequest("Course cannot be null.");
            }

            if (course.Id == Guid.Empty)
            {
                return Result<Course>.BadRequest("Course ID cannot be empty.");
            }

            var existingCourse = await _courseRepository.GetByIdAsync(course.Id, cancellationToken);
            if (existingCourse == null)
            {
                return Result<Course>.NotFound($"Course with ID '{course.Id}' not found.");
            }

            existingCourse.Update(
                course.Title,
                course.Description,
                course.DurationInDays
            );

            var updatedCourse = await _courseRepository.UpdateAsync(existingCourse.Id, existingCourse, cancellationToken);

            if (updatedCourse == null)
            {
                return Result<Course>.Error("Failed to update course.");
            }

            return Result<Course>.Ok(updatedCourse);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("modified by another user"))
        {
            return Result<Course>.Conflict("The course was modified by another user. Please refresh and try again.");
        }
        catch (ArgumentException ex)
        {
            return Result<Course>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<Course>.Error("An error occurred while updating the course.");
        }
    }

    public async Task<Result<bool>> DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseId == Guid.Empty)
            {
                return Result<bool>.BadRequest("Course ID cannot be empty.");
            }

            var existingCourse = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
            if (existingCourse == null)
            {
                return Result<bool>.NotFound($"Course with ID '{courseId}' not found.");
            }

            var hasCourseEvents = await _courseRepository.HasCourseEventsAsync(courseId, cancellationToken);
            if (hasCourseEvents)
            {
                return Result<bool>.Conflict($"Cannot delete course with ID '{courseId}' because it has associated course events. Please delete the course events first.");
            }

            var isDeleted = await _courseRepository.RemoveAsync(courseId, cancellationToken);

            if (!isDeleted)
            {
                return Result<bool>.Error("Failed to delete course.");
            }

            return Result<bool>.Ok(true);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("associated course events"))
        {
            return Result<bool>.Conflict("Cannot delete course because it has associated course events. Please delete the course events first.");
        }
        catch (Exception)
        {
            return Result<bool>.Error("An error occurred while deleting the course.");
        }
    }
}

