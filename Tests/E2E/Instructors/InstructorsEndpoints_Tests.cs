using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Application.Modules.Instructors.Outputs;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Application.Common;
using Backend.Presentation.API.Models.Instructor;
using Backend.Tests.Integration.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Backend.Domain.Modules.Instructors.Models;

namespace Backend.Tests.E2E.Instructors;

public sealed class InstructorsEndpoints_Tests(CoursesOnlineDbApiFactory factory) : IClassFixture<CoursesOnlineDbApiFactory>
{
    private readonly CoursesOnlineDbApiFactory _factory = factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetAllInstructors_ReturnsOk_WithEmptyList_AfterReset()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/instructors");
        var payload = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<Instructor>>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload.Success);
        Assert.NotNull(payload.Value);
        Assert.Empty(payload.Value);
    }

    [Fact]
    public async Task GetAllInstructors_ReturnsDescending_ById()
    {
        await _factory.ResetAndSeedDataAsync();

        int instructorRoleId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            instructorRoleId = (await RepositoryTestDataHelper.CreateInstructorRoleAsync(db)).Id;
        }

        using var client = _factory.CreateClient();
        _ = await client.PostAsJsonAsync("/api/instructors", new CreateInstructorRequest
        {
            Name = $"Order-A-{Guid.NewGuid():N}",
            InstructorRoleId = instructorRoleId
        });
        _ = await client.PostAsJsonAsync("/api/instructors", new CreateInstructorRequest
        {
            Name = $"Order-B-{Guid.NewGuid():N}",
            InstructorRoleId = instructorRoleId
        });

        var response = await client.GetAsync("/api/instructors");
        var payload = await response.Content.ReadFromJsonAsync<JsonDocument>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        var ids = payload.RootElement
            .GetProperty("value")
            .EnumerateArray()
            .Select(item => item.GetProperty("id").GetGuid())
            .ToList();
        Assert.True(ids.Count >= 2);

        for (var i = 1; i < ids.Count; i++)
        {
            Assert.True(ids[i - 1].CompareTo(ids[i]) >= 0);
        }
    }

    [Fact]
    public async Task CreateInstructor_ThenGetById_ReturnsCreatedInstructor()
    {
        await _factory.ResetAndSeedDataAsync();

        int instructorRoleId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            instructorRoleId = (await RepositoryTestDataHelper.CreateInstructorRoleAsync(db)).Id;
        }

        using var client = _factory.CreateClient();
        var createRequest = new CreateInstructorRequest
        {
            Name = "Donald Knuth",
            InstructorRoleId = instructorRoleId
        };

        var createResponse = await client.PostAsJsonAsync("/api/instructors", createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var createdInstructorId = Guid.Parse(createResponse.Headers.Location!.OriginalString.Split('/')[^1]);
        var getResponse = await client.GetAsync($"/api/instructors/{createdInstructorId}");
        var getPayload = await getResponse.Content.ReadFromJsonAsync<Result<InstructorDetails>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(getPayload);
        Assert.True(getPayload.Success);
        Assert.NotNull(getPayload.Value);
        Assert.Equal(createdInstructorId, getPayload.Value.Id);
        Assert.Equal("Donald Knuth", getPayload.Value.Name);
        Assert.Equal(instructorRoleId, getPayload.Value.InstructorRole.Id);
    }

    [Fact]
    public async Task CreateInstructor_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var createRequest = new CreateInstructorRequest
        {
            Name = "Invalid Role Instructor",
            InstructorRoleId = int.MaxValue
        };

        var response = await client.PostAsJsonAsync("/api/instructors", createRequest);
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.NotFound, payload.ErrorType);
    }

    [Fact]
    public async Task GetInstructorById_ReturnsBadRequest_ForEmptyGuid()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/instructors/{Guid.Empty}");
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.BadRequest, payload.ErrorType);
    }

    [Fact]
    public async Task DeleteInstructor_ReturnsConflict_WhenInstructorHasCourseEvents()
    {
        await _factory.ResetAndSeedDataAsync();

        Guid instructorId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            var instructor = await RepositoryTestDataHelper.CreateInstructorAsync(db);
            var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(db, seats: 10);
            await RepositoryTestDataHelper.LinkInstructorToCourseEventAsync(db, instructor.Id, courseEvent.Id);
            instructorId = instructor.Id;
        }

        using var client = _factory.CreateClient();
        var response = await client.DeleteAsync($"/api/instructors/{instructorId}");
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.Conflict, payload.ErrorType);    }

    [Fact]
    public async Task DeleteInstructor_ReturnsOk_AndRemovesInstructor()
    {
        await _factory.ResetAndSeedDataAsync();

        Guid instructorId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            instructorId = (await RepositoryTestDataHelper.CreateInstructorAsync(db)).Id;
        }

        using var client = _factory.CreateClient();

        var deleteResponse = await client.DeleteAsync($"/api/instructors/{instructorId}");
        var deletePayload = await deleteResponse.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.NotNull(deletePayload);
        Assert.True(deletePayload.Success);
        var getResponse = await client.GetAsync($"/api/instructors/{instructorId}");
        var getPayload = await getResponse.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.NotNull(getPayload);
        Assert.False(getPayload.Success);
        Assert.Equal(ErrorTypes.NotFound, getPayload.ErrorType);
    }
}
