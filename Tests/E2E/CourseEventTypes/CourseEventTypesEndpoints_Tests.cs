using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Application.Common;
using Backend.Presentation.API.Models.CourseEventType;
using Backend.Tests.Integration.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Tests.E2E.CourseEventTypes;

public sealed class CourseEventTypesEndpoints_Tests(CoursesOnlineDbApiFactory factory) : IClassFixture<CoursesOnlineDbApiFactory>
{
    private sealed record CourseEventTypeDto(int Id, string Name);

    private readonly CoursesOnlineDbApiFactory _factory = factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetAllCourseEventTypes_ReturnsOk_WithListPayload_AfterReset()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/course-event-types");
        var payload = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<CourseEventTypeDto>>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload.Success);
        Assert.NotNull(payload.Value);
    }

    [Fact]
    public async Task GetAllCourseEventTypes_ReturnsNewestFirst_ByIdDescending()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var firstCreate = await client.PostAsJsonAsync("/api/course-event-types", new CreateCourseEventTypeRequest
        {
            Name = $"OrderA-{Guid.NewGuid():N}"
        });
        var firstId = int.Parse(firstCreate.Headers.Location!.OriginalString.Split('/')[^1]);

        var secondCreate = await client.PostAsJsonAsync("/api/course-event-types", new CreateCourseEventTypeRequest
        {
            Name = $"OrderB-{Guid.NewGuid():N}"
        });
        var secondId = int.Parse(secondCreate.Headers.Location!.OriginalString.Split('/')[^1]);

        var response = await client.GetAsync("/api/course-event-types");
        var payload = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<CourseEventTypeDto>>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload?.Value);
        Assert.Equal(secondId, payload.Value[0].Id);
        Assert.Equal(firstId, payload.Value[1].Id);
    }

    [Fact]
    public async Task CreateCourseEventType_ThenGetById_ReturnsCreatedType()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var createRequest = new CreateCourseEventTypeRequest
        {
            Name = $"Webinar-{Guid.NewGuid():N}"
        };

        var createResponse = await client.PostAsJsonAsync("/api/course-event-types", createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var createdId = int.Parse(createResponse.Headers.Location!.OriginalString.Split('/')[^1]);
        var getResponse = await client.GetAsync($"/api/course-event-types/{createdId}");
        var getPayload = await getResponse.Content.ReadFromJsonAsync<Result<CourseEventTypeDto>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(getPayload);
        Assert.True(getPayload.Success);
        Assert.NotNull(getPayload.Value);
        Assert.Equal(createdId, getPayload.Value.Id);
        Assert.Equal(createRequest.Name, getPayload.Value.Name);
    }

    [Fact]
    public async Task CreateCourseEventType_ReturnsBadRequest_ForInvalidPayload()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        using var content = new StringContent("{\"name\":123}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/course-event-types", content);
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
    }

    [Fact]
    public async Task GetCourseEventTypeById_ReturnsBadRequest_ForInvalidId()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/course-event-types/0");
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.BadRequest, payload.ErrorType);
    }

    [Fact]
    public async Task DeleteCourseEventType_ReturnsConflict_WhenTypeIsInUse()
    {
        await _factory.ResetAndSeedDataAsync();

        int eventTypeId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            eventTypeId = (await RepositoryTestDataHelper.CreateCourseEventTypeAsync(db)).Id;
            _ = await RepositoryTestDataHelper.CreateCourseEventAsync(db, typeId: eventTypeId);
        }

        using var client = _factory.CreateClient();
        var response = await client.DeleteAsync($"/api/course-event-types/{eventTypeId}");
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.Conflict, payload.ErrorType);    }

    [Fact]
    public async Task DeleteCourseEventType_ReturnsOk_AndRemovesType()
    {
        await _factory.ResetAndSeedDataAsync();

        int eventTypeId;
        using (var client = _factory.CreateClient())
        {
            var createRequest = new CreateCourseEventTypeRequest
            {
                Name = $"TempType-{Guid.NewGuid():N}"
            };

            var createResponse = await client.PostAsJsonAsync("/api/course-event-types", createRequest);
            eventTypeId = int.Parse(createResponse.Headers.Location!.OriginalString.Split('/')[^1]);
        }

        using var verificationClient = _factory.CreateClient();

        var deleteResponse = await verificationClient.DeleteAsync($"/api/course-event-types/{eventTypeId}");
        var deletePayload = await deleteResponse.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.NotNull(deletePayload);
        Assert.True(deletePayload.Success);
        var getResponse = await verificationClient.GetAsync($"/api/course-event-types/{eventTypeId}");
        var getPayload = await getResponse.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.NotNull(getPayload);
        Assert.False(getPayload.Success);
        Assert.Equal(ErrorTypes.NotFound, getPayload.ErrorType);
    }
}
