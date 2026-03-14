using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Application.Modules.CourseRegistrations.Outputs;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Presentation.API.Models.CourseRegistration;
using Backend.Application.Common;
using Microsoft.Extensions.DependencyInjection;
using Backend.Tests.Integration.Infrastructure;
using Backend.Domain.Modules.CourseRegistrations.Models;

namespace Backend.Tests.E2E.CourseRegistrations;

public sealed class CourseRegistrationsEndpoints_Tests(CoursesOnlineDbApiFactory factory) : IClassFixture<CoursesOnlineDbApiFactory>
{
    private readonly CoursesOnlineDbApiFactory _factory = factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetAllCourseRegistrations_ReturnsOk_WithEmptyList_AfterReset()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/course-registrations");
        var payload = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<CourseRegistration>>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload.Success);
        Assert.NotNull(payload.Value);
        Assert.Empty(payload.Value);
    }

    [Fact]
    public async Task GetAllCourseRegistrations_ReturnsNewestFirst_ByRegistrationDateDescending()
    {
        await _factory.ResetAndSeedDataAsync();

        Guid participantId;
        Guid secondParticipantId;
        Guid courseEventId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            participantId = (await RepositoryTestDataHelper.CreateParticipantAsync(db)).Id;
            secondParticipantId = (await RepositoryTestDataHelper.CreateParticipantAsync(db)).Id;
            courseEventId = (await RepositoryTestDataHelper.CreateCourseEventAsync(db, seats: 10)).Id;
        }

        using var client = _factory.CreateClient();
        var firstCreate = await client.PostAsJsonAsync("/api/course-registrations", new CreateCourseRegistrationRequest
        {
            ParticipantId = participantId,
            CourseEventId = courseEventId,
            StatusId = 1,
            PaymentMethodId = 1
        });
        var firstId = Guid.Parse(firstCreate.Headers.Location!.OriginalString.Split('/')[^1]);

        var secondCreate = await client.PostAsJsonAsync("/api/course-registrations", new CreateCourseRegistrationRequest
        {
            ParticipantId = secondParticipantId,
            CourseEventId = courseEventId,
            StatusId = 1,
            PaymentMethodId = 1
        });
        var secondId = Guid.Parse(secondCreate.Headers.Location!.OriginalString.Split('/')[^1]);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            var firstEntity = await db.CourseRegistrations.FindAsync(firstId);
            var secondEntity = await db.CourseRegistrations.FindAsync(secondId);
            Assert.NotNull(firstEntity);
            Assert.NotNull(secondEntity);
            firstEntity!.RegistrationDate = DateTime.UtcNow.AddMinutes(-2);
            secondEntity!.RegistrationDate = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/course-registrations");
        var payload = await response.Content.ReadFromJsonAsync<JsonDocument>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        var ids = payload.RootElement
            .GetProperty("value")
            .EnumerateArray()
            .Select(item => item.GetProperty("id").GetGuid())
            .ToList();

        var firstIndex = ids.FindIndex(x => x == firstId);
        var secondIndex = ids.FindIndex(x => x == secondId);

        Assert.True(firstIndex >= 0);
        Assert.True(secondIndex >= 0);
        Assert.True(secondIndex < firstIndex);
    }

    [Fact]
    public async Task CreateCourseRegistration_ThenGetById_ReturnsCreatedRegistration()
    {
        await _factory.ResetAndSeedDataAsync();

        Guid participantId;
        Guid courseEventId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            participantId = (await RepositoryTestDataHelper.CreateParticipantAsync(db)).Id;
            courseEventId = (await RepositoryTestDataHelper.CreateCourseEventAsync(db, seats: 10)).Id;
        }

        using var client = _factory.CreateClient();
        var createRequest = new CreateCourseRegistrationRequest
        {
            ParticipantId = participantId,
            CourseEventId = courseEventId,
            StatusId = 1,
            PaymentMethodId = 1
        };

        var createResponse = await client.PostAsJsonAsync("/api/course-registrations", createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var createdRegistrationId = Guid.Parse(createResponse.Headers.Location!.OriginalString.Split('/')[^1]);
        var getResponse = await client.GetAsync($"/api/course-registrations/{createdRegistrationId}");
        var getPayload = await getResponse.Content.ReadFromJsonAsync<Result<CourseRegistrationDetails>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(getPayload);
        Assert.True(getPayload.Success);
        Assert.NotNull(getPayload.Value);
        Assert.Equal(createdRegistrationId, getPayload.Value.Id);
        Assert.Equal(participantId, getPayload.Value.Participant.Id);
        Assert.Equal(courseEventId, getPayload.Value.CourseEvent.Id);
    }

    [Fact]
    public async Task CreateCourseRegistration_ReturnsBadRequest_ForNegativeStatusId()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var createRequest = new CreateCourseRegistrationRequest
        {
            ParticipantId = Guid.NewGuid(),
            CourseEventId = Guid.NewGuid(),
            StatusId = -1,
            PaymentMethodId = 1
        };

        var response = await client.PostAsJsonAsync("/api/course-registrations", createRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCourseRegistration_ReturnsNotFound_WhenParticipantDoesNotExist()
    {
        await _factory.ResetAndSeedDataAsync();

        Guid courseEventId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            courseEventId = (await RepositoryTestDataHelper.CreateCourseEventAsync(db, seats: 10)).Id;
        }

        using var client = _factory.CreateClient();
        var createRequest = new CreateCourseRegistrationRequest
        {
            ParticipantId = Guid.NewGuid(),
            CourseEventId = courseEventId,
            StatusId = 1,
            PaymentMethodId = 1
        };

        var response = await client.PostAsJsonAsync("/api/course-registrations", createRequest);
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.NotFound, payload.ErrorType);
    }

    [Fact]
    public async Task GetCourseRegistrationById_ReturnsBadRequest_ForEmptyGuid()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/course-registrations/{Guid.Empty}");
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.BadRequest, payload.ErrorType);
    }

    [Fact]
    public async Task DeleteCourseRegistration_ReturnsNotFound_WhenRegistrationDoesNotExist()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/api/course-registrations/{Guid.NewGuid()}");
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.NotFound, payload.ErrorType);
    }

    [Fact]
    public async Task DeleteCourseRegistration_ReturnsOk_AndRemovesRegistration()
    {
        await _factory.ResetAndSeedDataAsync();

        Guid registrationId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            var participant = await RepositoryTestDataHelper.CreateParticipantAsync(db);
            var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(db, seats: 5);
            var registration = await RepositoryTestDataHelper.CreateCourseRegistrationAsync(
                db,
                participantId: participant.Id,
                courseEventId: courseEvent.Id);
            registrationId = registration.Id;
        }

        using var client = _factory.CreateClient();

        var deleteResponse = await client.DeleteAsync($"/api/course-registrations/{registrationId}");
        var deletePayload = await deleteResponse.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.NotNull(deletePayload);
        Assert.True(deletePayload.Success);
        var getResponse = await client.GetAsync($"/api/course-registrations/{registrationId}");
        var getPayload = await getResponse.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.NotNull(getPayload);
        Assert.False(getPayload.Success);
        Assert.Equal(ErrorTypes.NotFound, getPayload.ErrorType);
    }
}

