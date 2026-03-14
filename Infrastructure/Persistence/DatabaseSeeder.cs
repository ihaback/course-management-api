using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(CoursesOnlineDbContext context, CancellationToken ct = default)
    {
        if (await context.VenueTypes.AnyAsync(ct))
            return;

        await SeedFixedLookupsAsync(context, ct);

        var roles = await SeedInstructorRolesAsync(context, ct);
        var eventTypes = await SeedCourseEventTypesAsync(context, ct);
        var locations = await SeedLocationsAsync(context, ct);
        var inPlaceLocations = await SeedInPlaceLocationsAsync(context, locations, ct);
        var courses = await SeedCoursesAsync(context, ct);
        var instructors = await SeedInstructorsAsync(context, roles, ct);
        var participants = await SeedParticipantsAsync(context, ct);
        var courseEvents = await SeedCourseEventsAsync(context, courses, eventTypes, instructors, inPlaceLocations, ct);
        await SeedCourseRegistrationsAsync(context, participants, courseEvents, ct);
    }

    private static async Task SeedFixedLookupsAsync(CoursesOnlineDbContext context, CancellationToken ct)
    {
        context.VenueTypes.AddRange(
            new VenueTypeEntity { Id = 1, Name = "InPerson" },
            new VenueTypeEntity { Id = 2, Name = "Online" },
            new VenueTypeEntity { Id = 3, Name = "Hybrid" });

        context.PaymentMethods.AddRange(
            new PaymentMethodEntity { Id = 1, Name = "Card" },
            new PaymentMethodEntity { Id = 2, Name = "Invoice" },
            new PaymentMethodEntity { Id = 3, Name = "Cash" });

        context.ParticipantContactTypes.AddRange(
            new ParticipantContactTypeEntity { Id = 1, Name = "Primary" },
            new ParticipantContactTypeEntity { Id = 2, Name = "Billing" },
            new ParticipantContactTypeEntity { Id = 3, Name = "Emergency" });

        context.CourseRegistrationStatuses.AddRange(
            new CourseRegistrationStatusEntity { Id = 0, Name = "Pending" },
            new CourseRegistrationStatusEntity { Id = 1, Name = "Paid" },
            new CourseRegistrationStatusEntity { Id = 2, Name = "Cancelled" },
            new CourseRegistrationStatusEntity { Id = 3, Name = "Refunded" });

        await context.SaveChangesAsync(ct);
    }

    private static async Task<List<InstructorRoleEntity>> SeedInstructorRolesAsync(CoursesOnlineDbContext context, CancellationToken ct)
    {
        var roles = new List<InstructorRoleEntity>
        {
            new() { Name = "Lead Instructor" },
            new() { Name = "Assistant Instructor" },
            new() { Name = "Guest Instructor" },
        };
        context.InstructorRoles.AddRange(roles);
        await context.SaveChangesAsync(ct);
        return roles;
    }

    private static async Task<List<CourseEventTypeEntity>> SeedCourseEventTypesAsync(CoursesOnlineDbContext context, CancellationToken ct)
    {
        var types = new List<CourseEventTypeEntity>
        {
            new() { Name = "Standard" },
            new() { Name = "Workshop" },
            new() { Name = "Webinar" },
        };
        context.CourseEventTypes.AddRange(types);
        await context.SaveChangesAsync(ct);
        return types;
    }

    private static async Task<List<LocationEntity>> SeedLocationsAsync(CoursesOnlineDbContext context, CancellationToken ct)
    {
        var locations = new List<LocationEntity>
        {
            new() { StreetName = "Drottninggatan 95", PostalCode = "11360", City = "Stockholm" },
            new() { StreetName = "Kungsgatan 12", PostalCode = "41119", City = "Gothenburg" },
            new() { StreetName = "Stortorget 7", PostalCode = "21134", City = "Malmo" },
        };
        context.Locations.AddRange(locations);
        await context.SaveChangesAsync(ct);
        return locations;
    }

    private static async Task<List<InPlaceLocationEntity>> SeedInPlaceLocationsAsync(
        CoursesOnlineDbContext context, List<LocationEntity> locations, CancellationToken ct)
    {
        var inPlaceLocations = new List<InPlaceLocationEntity>
        {
            new() { LocationId = locations[0].Id, RoomNumber = 101, Seats = 25 },
            new() { LocationId = locations[0].Id, RoomNumber = 102, Seats = 30 },
            new() { LocationId = locations[1].Id, RoomNumber = 101, Seats = 22 },
            new() { LocationId = locations[2].Id, RoomNumber = 201, Seats = 24 },
        };
        context.InPlaceLocations.AddRange(inPlaceLocations);
        await context.SaveChangesAsync(ct);
        return inPlaceLocations;
    }

    private static async Task<List<CourseEntity>> SeedCoursesAsync(CoursesOnlineDbContext context, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var courses = new List<CourseEntity>
        {
            new() { Id = Guid.NewGuid(), Title = "Introduction to C# Programming", Description = "Learn the fundamentals of C# and object-oriented programming concepts.", DurationInDays = 3, CreatedAtUtc = now, ModifiedAtUtc = now },
            new() { Id = Guid.NewGuid(), Title = "Advanced ASP.NET Core Development", Description = "Master advanced ASP.NET Core concepts and build REST APIs.", DurationInDays = 4, CreatedAtUtc = now, ModifiedAtUtc = now },
            new() { Id = Guid.NewGuid(), Title = "Database Design with Entity Framework", Description = "Database design, SQL, and Entity Framework Core ORM.", DurationInDays = 3, CreatedAtUtc = now, ModifiedAtUtc = now },
            new() { Id = Guid.NewGuid(), Title = "React and TypeScript Fundamentals", Description = "Build modern web applications using React and TypeScript.", DurationInDays = 4, CreatedAtUtc = now, ModifiedAtUtc = now },
            new() { Id = Guid.NewGuid(), Title = "Cloud Architecture with Azure", Description = "Design and implement scalable solutions with Azure services.", DurationInDays = 5, CreatedAtUtc = now, ModifiedAtUtc = now },
        };
        context.Courses.AddRange(courses);
        await context.SaveChangesAsync(ct);
        return courses;
    }

    private static async Task<List<InstructorEntity>> SeedInstructorsAsync(CoursesOnlineDbContext context, List<InstructorRoleEntity> roles, CancellationToken ct)
    {
        var instructors = new List<InstructorEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Erik Lindqvist", InstructorRoleId = roles[0].Id },
            new() { Id = Guid.NewGuid(), Name = "Anna Johansson", InstructorRoleId = roles[1].Id },
            new() { Id = Guid.NewGuid(), Name = "Lars Eriksson", InstructorRoleId = roles[2].Id },
            new() { Id = Guid.NewGuid(), Name = "Maria Svensson", InstructorRoleId = roles[2].Id },
            new() { Id = Guid.NewGuid(), Name = "Johan Karlsson", InstructorRoleId = roles[0].Id },
        };
        context.Instructors.AddRange(instructors);
        await context.SaveChangesAsync(ct);
        return instructors;
    }

    private static async Task<List<ParticipantEntity>> SeedParticipantsAsync(CoursesOnlineDbContext context, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var participants = new List<ParticipantEntity>
        {
            new() { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Karlsson", Email = "alice.karlsson@example.com", PhoneNumber = "+46701234567", ContactTypeId = 1, CreatedAtUtc = now, ModifiedAtUtc = now },
            new() { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Andersson", Email = "bob.andersson@example.com", PhoneNumber = "+46702345678", ContactTypeId = 1, CreatedAtUtc = now, ModifiedAtUtc = now },
            new() { Id = Guid.NewGuid(), FirstName = "Charlie", LastName = "Johansson", Email = "charlie.johansson@example.com", PhoneNumber = "+46703456789", ContactTypeId = 1, CreatedAtUtc = now, ModifiedAtUtc = now },
            new() { Id = Guid.NewGuid(), FirstName = "Diana", LastName = "Eriksson", Email = "diana.eriksson@example.com", PhoneNumber = "+46704567890", ContactTypeId = 1, CreatedAtUtc = now, ModifiedAtUtc = now },
            new() { Id = Guid.NewGuid(), FirstName = "Erik", LastName = "Larsson", Email = "erik.larsson@example.com", PhoneNumber = "+46705678901", ContactTypeId = 1, CreatedAtUtc = now, ModifiedAtUtc = now },
        };
        context.Participants.AddRange(participants);
        await context.SaveChangesAsync(ct);
        return participants;
    }

    private static async Task<List<CourseEventEntity>> SeedCourseEventsAsync(
        CoursesOnlineDbContext context,
        List<CourseEntity> courses,
        List<CourseEventTypeEntity> eventTypes,
        List<InstructorEntity> instructors,
        List<InPlaceLocationEntity> inPlaceLocations,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var courseEvents = new List<CourseEventEntity>
        {
            new() { Id = Guid.NewGuid(), CourseId = courses[0].Id, EventDate = now.AddDays(10),  Price = 4500m, Seats = 20, CourseEventTypeId = eventTypes[0].Id, VenueTypeId = 2, CreatedAtUtc = now, ModifiedAtUtc = now },
            new() { Id = Guid.NewGuid(), CourseId = courses[1].Id, EventDate = now.AddDays(15),  Price = 7500m, Seats = 15, CourseEventTypeId = eventTypes[2].Id, VenueTypeId = 2, CreatedAtUtc = now, ModifiedAtUtc = now },
            new() { Id = Guid.NewGuid(), CourseId = courses[2].Id, EventDate = now.AddDays(20),  Price = 5500m, Seats = 22, CourseEventTypeId = eventTypes[1].Id, VenueTypeId = 1, CreatedAtUtc = now, ModifiedAtUtc = now },
            new() { Id = Guid.NewGuid(), CourseId = courses[3].Id, EventDate = now.AddDays(25),  Price = 6500m, Seats = 16, CourseEventTypeId = eventTypes[2].Id, VenueTypeId = 2, CreatedAtUtc = now, ModifiedAtUtc = now },
            new() { Id = Guid.NewGuid(), CourseId = courses[4].Id, EventDate = now.AddDays(30),  Price = 8500m, Seats = 12, CourseEventTypeId = eventTypes[0].Id, VenueTypeId = 3, CreatedAtUtc = now, ModifiedAtUtc = now },
        };

        courseEvents[0].Instructors.Add(instructors[0]);
        courseEvents[1].Instructors.Add(instructors[1]);
        courseEvents[2].Instructors.Add(instructors[2]);
        courseEvents[3].Instructors.Add(instructors[3]);
        courseEvents[4].Instructors.Add(instructors[4]);

        // In-person events get a room assignment
        courseEvents[2].InPlaceLocations.Add(inPlaceLocations[0]);
        courseEvents[4].InPlaceLocations.Add(inPlaceLocations[2]);

        context.CourseEvents.AddRange(courseEvents);
        await context.SaveChangesAsync(ct);
        return courseEvents;
    }

    private static async Task SeedCourseRegistrationsAsync(
        CoursesOnlineDbContext context,
        List<ParticipantEntity> participants,
        List<CourseEventEntity> courseEvents,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        context.CourseRegistrations.AddRange(
            new CourseRegistrationEntity { Id = Guid.NewGuid(), ParticipantId = participants[0].Id, CourseEventId = courseEvents[0].Id, RegistrationDate = now.AddDays(-5), CourseRegistrationStatusId = 1, PaymentMethodId = 1, ModifiedAtUtc = now },
            new CourseRegistrationEntity { Id = Guid.NewGuid(), ParticipantId = participants[1].Id, CourseEventId = courseEvents[0].Id, RegistrationDate = now.AddDays(-4), CourseRegistrationStatusId = 0, PaymentMethodId = 2, ModifiedAtUtc = now },
            new CourseRegistrationEntity { Id = Guid.NewGuid(), ParticipantId = participants[2].Id, CourseEventId = courseEvents[1].Id, RegistrationDate = now.AddDays(-3), CourseRegistrationStatusId = 1, PaymentMethodId = 1, ModifiedAtUtc = now },
            new CourseRegistrationEntity { Id = Guid.NewGuid(), ParticipantId = participants[3].Id, CourseEventId = courseEvents[2].Id, RegistrationDate = now.AddDays(-2), CourseRegistrationStatusId = 0, PaymentMethodId = 3, ModifiedAtUtc = now });
        await context.SaveChangesAsync(ct);
    }
}
