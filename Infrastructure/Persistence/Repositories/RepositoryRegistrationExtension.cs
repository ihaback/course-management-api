using Backend.Domain.Modules.CourseEvents.Contracts;
using Backend.Domain.Modules.CourseEventTypes.Contracts;
using Backend.Domain.Modules.CourseRegistrations.Contracts;
using Backend.Domain.Modules.CourseRegistrationStatuses.Contracts;
using Backend.Domain.Modules.Courses.Contracts;
using Backend.Domain.Modules.InPlaceLocations.Contracts;
using Backend.Domain.Modules.InstructorRoles.Contracts;
using Backend.Domain.Modules.Instructors.Contracts;
using Backend.Domain.Modules.Locations.Contracts;
using Backend.Domain.Modules.ParticipantContactTypes.Contracts;
using Backend.Domain.Modules.Participants.Contracts;
using Backend.Domain.Modules.PaymentMethods.Contracts;
using Backend.Domain.Modules.VenueTypes.Contracts;
using Backend.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infrastructure.Persistence.Repositories;

public static class RepositoryRegistrationExtension
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ICourseEventRepository, CourseEventRepository>();
        services.AddScoped<ICourseEventTypeRepository, CourseEventTypeRepository>();
        services.AddScoped<ICourseRegistrationRepository, CourseRegistrationRepository>();
        services.AddScoped<ICourseRegistrationStatusRepository, CourseRegistrationStatusRepository>();
        services.AddScoped<IParticipantRepository, ParticipantRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IInPlaceLocationRepository, InPlaceLocationRepository>();
        services.AddScoped<IInstructorRepository, InstructorRepository>();
        services.AddScoped<IInstructorRoleRepository, InstructorRoleRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<IVenueTypeRepository, VenueTypeRepository>();
        services.AddScoped<IParticipantContactTypeRepository, ParticipantContactTypeRepository>();

        return services;
    }
}
