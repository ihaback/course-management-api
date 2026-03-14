using Backend.Domain.Modules.CourseEvents.Models;
using Backend.Domain.Modules.CourseEventTypes.Models;
using Backend.Domain.Modules.CourseRegistrations.Models;
using Backend.Domain.Modules.CourseRegistrationStatuses.Models;
using Backend.Domain.Modules.Courses.Models;
using Backend.Domain.Modules.InPlaceLocations.Models;
using Backend.Domain.Modules.InstructorRoles.Models;
using Backend.Domain.Modules.Instructors.Models;
using Backend.Domain.Modules.Locations.Models;
using Backend.Domain.Modules.Participants.Models;
using Backend.Domain.Modules.PaymentMethods.Models;
using Backend.Domain.Modules.VenueTypes.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Integration.Infrastructure;

internal static class RepositoryTestDataHelper
{
    public static Task<Course> CreateCourseAsync(CoursesOnlineDbContext context)
        => new CourseRepository(context).AddAsync(
            Course.Reconstitute(
                Guid.NewGuid(),
                $"Course-{Guid.NewGuid():N}",
                "Integration course",
                3),
            CancellationToken.None);

    public static Task<CourseEventType> CreateCourseEventTypeAsync(CoursesOnlineDbContext context)
        => new CourseEventTypeRepository(context).AddAsync(
            CourseEventType.Create($"Type-{Guid.NewGuid():N}"),
            CancellationToken.None);

    public static async Task<CourseEvent> CreateCourseEventAsync(CoursesOnlineDbContext context, Guid? courseId = null, int? typeId = null, int seats = 5)
    {
        var course = courseId.HasValue
            ? await new CourseRepository(context).GetByIdWithEventsAsync(courseId.Value, CancellationToken.None)
            : null;
        var eventType = typeId.HasValue
            ? await new CourseEventTypeRepository(context).GetByIdAsync(typeId.Value, CancellationToken.None)
            : null;

        var resolvedCourseId = course?.Course.Id ?? (await CreateCourseAsync(context)).Id;
        var resolvedTypeId = eventType?.Id ?? (await CreateCourseEventTypeAsync(context)).Id;

        return await new CourseEventRepository(context).AddAsync(
            CourseEvent.Reconstitute(
                Guid.NewGuid(),
                resolvedCourseId,
                DateTime.UtcNow.AddDays(1),
                99m,
                seats,
                resolvedTypeId,
                VenueType.Reconstitute(1, "InPerson")),
            CancellationToken.None);
    }

    public static Task<Participant> CreateParticipantAsync(CoursesOnlineDbContext context)
        => new ParticipantRepository(context).AddAsync(
            Participant.Reconstitute(
                Guid.NewGuid(),
                "First",
                "Last",
                $"participant-{Guid.NewGuid():N}@example.com",
                "123456789"),
            CancellationToken.None);

    public static Task<Location> CreateLocationAsync(CoursesOnlineDbContext context)
        => new LocationRepository(context).AddAsync(
            Location.Create($"Street-{Guid.NewGuid():N}", "12345", "City"),
            CancellationToken.None);

    public static async Task<InPlaceLocation> CreateInPlaceLocationAsync(CoursesOnlineDbContext context, int? locationId = null)
    {
        var resolvedLocationId = locationId ?? (await CreateLocationAsync(context)).Id;
        return await new InPlaceLocationRepository(context).AddAsync(
            InPlaceLocation.Create(resolvedLocationId, 101, 20),
            CancellationToken.None);
    }

    public static Task<InstructorRole> CreateInstructorRoleAsync(CoursesOnlineDbContext context)
        => new InstructorRoleRepository(context).AddAsync(
            InstructorRole.Create($"Role-{Guid.NewGuid():N}"),
            CancellationToken.None);

    public static async Task<Instructor> CreateInstructorAsync(CoursesOnlineDbContext context, int? roleId = null)
    {
        var role = roleId.HasValue
            ? await new InstructorRoleRepository(context).GetByIdAsync(roleId.Value, CancellationToken.None)
            : await CreateInstructorRoleAsync(context);

        return await new InstructorRepository(context).AddAsync(
            Instructor.Reconstitute(Guid.NewGuid(), $"Instructor-{Guid.NewGuid():N}", role!),
            CancellationToken.None);
    }

    public static async Task<CourseRegistration> CreateCourseRegistrationAsync(
        CoursesOnlineDbContext context,
        Guid? participantId = null,
        Guid? courseEventId = null,
        CourseRegistrationStatus? status = null)
    {
        var resolvedParticipantId = participantId ?? (await CreateParticipantAsync(context)).Id;
        var resolvedEventId = courseEventId ?? (await CreateCourseEventAsync(context)).Id;
        var resolvedStatus = status ?? CourseRegistrationStatus.Pending;

        return await new CourseRegistrationRepository(context).AddAsync(
            CourseRegistration.Reconstitute(
                Guid.NewGuid(),
                resolvedParticipantId,
                resolvedEventId,
                DateTime.UtcNow,
                resolvedStatus,
                PaymentMethod.Reconstitute(1, "Card")),
            CancellationToken.None);
    }

    public static async Task LinkInstructorToCourseEventAsync(CoursesOnlineDbContext context, Guid instructorId, Guid eventId)
    {
        var instructor = await context.Instructors.SingleAsync(x => x.Id == instructorId);
        var courseEvent = await context.CourseEvents.SingleAsync(x => x.Id == eventId);
        courseEvent.Instructors.Add(instructor);
        await context.SaveChangesAsync();
    }

    public static async Task LinkInPlaceLocationToCourseEventAsync(CoursesOnlineDbContext context, int inPlaceLocationId, Guid eventId)
    {
        var inPlace = await context.InPlaceLocations.SingleAsync(x => x.Id == inPlaceLocationId);
        var courseEvent = await context.CourseEvents.SingleAsync(x => x.Id == eventId);
        courseEvent.InPlaceLocations.Add(inPlace);
        await context.SaveChangesAsync();
    }
}






