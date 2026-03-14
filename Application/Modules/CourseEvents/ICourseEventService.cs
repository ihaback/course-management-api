using Backend.Application.Common;
using Backend.Application.Modules.CourseEvents.Inputs;
using Backend.Application.Modules.CourseEvents.Outputs;
using Backend.Domain.Modules.CourseEvents.Models;

namespace Backend.Application.Modules.CourseEvents
{
    public interface ICourseEventService
    {
        Task<Result<CourseEvent>> CreateCourseEventAsync(CreateCourseEventInput courseEvent, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<CourseEvent>>> GetAllCourseEventsAsync(CancellationToken cancellationToken = default);
        Task<Result<CourseEventDetails>> GetCourseEventByIdAsync(Guid courseEventId, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<CourseEvent>>> GetCourseEventsByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);
        Task<Result<CourseEvent>> UpdateCourseEventAsync(UpdateCourseEventInput courseEvent, CancellationToken cancellationToken = default);
        Task<Result<bool>> DeleteCourseEventAsync(Guid courseEventId, CancellationToken cancellationToken = default);
    }
}
