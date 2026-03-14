using Backend.Application.Common;
using Backend.Application.Modules.CourseEventTypes.Inputs;
using Backend.Domain.Modules.CourseEventTypes.Models;

namespace Backend.Application.Modules.CourseEventTypes;

public interface ICourseEventTypeService
{
    Task<Result<CourseEventType>> CreateCourseEventTypeAsync(CreateCourseEventTypeInput courseEventType, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CourseEventType>>> GetAllCourseEventTypesAsync(CancellationToken cancellationToken = default);
    Task<Result<CourseEventType>> GetCourseEventTypeByIdAsync(int courseEventTypeId, CancellationToken cancellationToken = default);
    Task<Result<CourseEventType>> GetCourseEventTypeByTypeNameAsync(string typeName, CancellationToken cancellationToken = default);
    Task<Result<CourseEventType>> UpdateCourseEventTypeAsync(UpdateCourseEventTypeInput courseEventType, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteCourseEventTypeAsync(int courseEventTypeId, CancellationToken cancellationToken = default);
}
