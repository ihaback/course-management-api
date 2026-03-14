namespace Backend.Presentation.API.Endpoints;

public static class ApiEndpoints
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api")
            .AddEndpointFilterFactory(RequestValidationFilter.Factory);

        api.MapCoursesEndpoints();
        api.MapCourseEventsEndpoints();
        api.MapCourseEventTypesEndpoints();
        api.MapCourseRegistrationsEndpoints();
        api.MapCourseRegistrationStatusesEndpoints();
        api.MapParticipantContactTypesEndpoints();
        api.MapParticipantsEndpoints();
        api.MapPaymentMethodsEndpoints();
        api.MapVenueTypesEndpoints();
        api.MapLocationsEndpoints();
        api.MapInPlaceLocationsEndpoints();
        api.MapInstructorsEndpoints();
        api.MapInstructorRolesEndpoints();

        return app;
    }
}
