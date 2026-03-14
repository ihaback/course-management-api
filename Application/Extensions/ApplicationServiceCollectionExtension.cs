using Backend.Application.Modules.CourseEvents;
using Backend.Application.Modules.CourseEventTypes;
using Backend.Application.Modules.CourseEventTypes.Caching;
using Backend.Application.Modules.CourseRegistrations;
using Backend.Application.Modules.CourseRegistrationStatuses;
using Backend.Application.Modules.CourseRegistrationStatuses.Caching;
using Backend.Application.Modules.Courses;
using Backend.Application.Modules.InPlaceLocations;
using Backend.Application.Modules.InstructorRoles;
using Backend.Application.Modules.InstructorRoles.Caching;
using Backend.Application.Modules.Instructors;
using Backend.Application.Modules.Locations;
using Backend.Application.Modules.ParticipantContactTypes;
using Backend.Application.Modules.ParticipantContactTypes.Caching;
using Backend.Application.Modules.Participants;
using Backend.Application.Modules.PaymentMethods;
using Backend.Application.Modules.PaymentMethods.Caching;
using Backend.Application.Modules.VenueTypes;
using Backend.Application.Modules.VenueTypes.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Backend.Application.Extensions;

public static class ApplicationServiceCollectionExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ICourseEventService, CourseEventService>();
        services.AddScoped<ICourseEventTypeCache, CourseEventTypeCache>();
        services.AddScoped<ICourseEventTypeService, CourseEventTypeService>();
        services.AddScoped<ICourseRegistrationService, CourseRegistrationService>();
        services.AddScoped<ICourseRegistrationStatusCache, CourseRegistrationStatusCache>();
        services.AddScoped<ICourseRegistrationStatusService, CourseRegistrationStatusService>();
        services.AddScoped<IParticipantService, ParticipantService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IInPlaceLocationService, InPlaceLocationService>();
        services.AddScoped<IInstructorService, InstructorService>();
        services.AddScoped<IInstructorRoleCache, InstructorRoleCache>();
        services.AddScoped<IInstructorRoleService, InstructorRoleService>();
        services.AddScoped<IPaymentMethodCache, PaymentMethodCache>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<IVenueTypeCache, VenueTypeCache>();
        services.AddScoped<IVenueTypeService, VenueTypeService>();
        services.AddScoped<IParticipantContactTypeCache, ParticipantContactTypeCache>();
        services.AddScoped<IParticipantContactTypeService, ParticipantContactTypeService>();

        return services;
    }
}
