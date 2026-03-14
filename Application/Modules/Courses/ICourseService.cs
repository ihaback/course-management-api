using Backend.Application.Common;
using Backend.Application.Modules.Courses.Inputs;
using Backend.Domain.Modules.Courses.Models;

namespace Backend.Application.Modules.Courses
{
    public interface ICourseService
    {
        Task<Result<Course>> CreateCourseAsync(CreateCourseInput course, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<Course>>> GetAllCoursesAsync(CancellationToken cancellationToken = default);
        Task<Result<CourseWithEvents>> GetCourseByIdAsync(Guid courseId, CancellationToken cancellationToken = default);
        Task<Result<Course>> UpdateCourseAsync(UpdateCourseInput course, CancellationToken cancellationToken = default);
        Task<Result<bool>> DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken = default);
    }
}
