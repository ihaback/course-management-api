using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Presentation.API.Models.Course;
using Backend.Application.Common;
using Microsoft.Extensions.DependencyInjection;
using Backend.Tests.Integration.Infrastructure;
using Backend.Domain.Modules.Courses.Models;

namespace Backend.Tests.E2E.Courses;

public sealed class CoursesEndpoints_Tests(CoursesOnlineDbApiFactory factory) : IClassFixture<CoursesOnlineDbApiFactory>
{
    private readonly CoursesOnlineDbApiFactory _factory = factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetCourses_ReturnsOk_WithEmptyList_AfterReset()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/courses");
        var payload = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<Course>>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload.Success);
        Assert.NotNull(payload.Value);
        Assert.Empty(payload.Value);
    }

    [Fact]
    public async Task GetCourses_ReturnsNewestFirst_ByCreatedAtDescending()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var firstCreate = await client.PostAsJsonAsync("/api/courses", new CreateCourseRequest
        {
            Title = $"Order-A-{Guid.NewGuid():N}",
            Description = "Order test",
            DurationInDays = 2
        });
        var firstPayload = await firstCreate.Content.ReadFromJsonAsync<Result<Course>>(_jsonOptions);
        Assert.NotNull(firstPayload?.Value);

        var secondCreate = await client.PostAsJsonAsync("/api/courses", new CreateCourseRequest
        {
            Title = $"Order-B-{Guid.NewGuid():N}",
            Description = "Order test",
            DurationInDays = 2
        });
        var secondPayload = await secondCreate.Content.ReadFromJsonAsync<Result<Course>>(_jsonOptions);
        Assert.NotNull(secondPayload?.Value);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            var firstEntity = await db.Courses.FindAsync(firstPayload.Value.Id);
            var secondEntity = await db.Courses.FindAsync(secondPayload.Value.Id);
            Assert.NotNull(firstEntity);
            Assert.NotNull(secondEntity);
            firstEntity!.CreatedAtUtc = DateTime.UtcNow.AddMinutes(-2);
            secondEntity!.CreatedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/courses");
        var payload = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<Course>>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload?.Value);

        var firstIndex = payload.Value.ToList().FindIndex(x => x.Id == firstPayload.Value.Id);
        var secondIndex = payload.Value.ToList().FindIndex(x => x.Id == secondPayload.Value.Id);

        Assert.True(firstIndex >= 0);
        Assert.True(secondIndex >= 0);
        Assert.True(secondIndex < firstIndex);
    }

    [Fact]
    public async Task GetCourseById_ReturnsBadRequest_ForEmptyGuid()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/courses/{Guid.Empty}");
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.BadRequest, payload.ErrorType);
    }

    [Fact]
    public async Task GetCourseById_ReturnsNotFound_WhenCourseDoesNotExist()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/courses/{Guid.NewGuid()}");
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.NotFound, payload.ErrorType);
    }

    [Fact]
    public async Task CreateCourse_ReturnsBadRequest_ForWhitespaceTitle()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var request = new CreateCourseRequest
        {
            Title = "   ",
            Description = "Valid description",
            DurationInDays = 5
        };
        var response = await client.PostAsJsonAsync("/api/courses", request);
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.BadRequest, payload.ErrorType);
    }

    [Fact]
    public async Task CreateCourse_ReturnsBadRequest_ForInvalidDuration()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var request = new CreateCourseRequest
        {
            Title = "Valid title",
            Description = "Valid description",
            DurationInDays = 0
        };
        var response = await client.PostAsJsonAsync("/api/courses", request);
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.BadRequest, payload.ErrorType);
    }

    [Fact]
    public async Task CreateCourse_ReturnsBadRequest_ForInvalidJsonType()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var json = """
                   {
                       "title": 424324,
                       "description": "A description",
                       "durationInDays": 20
                   }
                   """;

        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/courses", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCourse_ReturnsNotFound_WhenCourseDoesNotExist()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var missingCourseId = Guid.NewGuid();
        var request = new UpdateCourseRequest
        {
            Title = "Updated title",
            Description = "Updated description",
            DurationInDays = 2
        };
        var response = await client.PutAsJsonAsync($"/api/courses/{missingCourseId}", request);
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.NotFound, payload.ErrorType);
    }

    [Fact]
    public async Task UpdateCourse_ReturnsBadRequest_ForInvalidDuration()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var createRequest = new CreateCourseRequest
        {
            Title = "Initial title",
            Description = "Initial description",
            DurationInDays = 3
        };
        var createResponse = await client.PostAsJsonAsync("/api/courses", createRequest);
        var createdPayload = await createResponse.Content.ReadFromJsonAsync<Result<Course>>(_jsonOptions);
        Assert.NotNull(createdPayload?.Value);

        var courseId = createdPayload.Value.Id;
        var updateRequest = new UpdateCourseRequest
        {
            Title = "Updated title",
            Description = "Updated description",
            DurationInDays = 0
        };
        var updateResponse = await client.PutAsJsonAsync($"/api/courses/{courseId}", updateRequest);
        var updatePayload = await updateResponse.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
        Assert.NotNull(updatePayload);
        Assert.False(updatePayload.Success);
        Assert.Equal(ErrorTypes.BadRequest, updatePayload.ErrorType);
    }

    [Fact]
    public async Task DeleteCourse_ReturnsBadRequest_ForEmptyGuid()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/api/courses/{Guid.Empty}");
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.BadRequest, payload.ErrorType);
    }

    [Fact]
    public async Task DeleteCourse_ReturnsNotFound_WhenCourseDoesNotExist()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/api/courses/{Guid.NewGuid()}");
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.NotFound, payload.ErrorType);
    }

    [Fact]
    public async Task DeleteCourse_ReturnsConflict_WhenCourseHasAssociatedEvents()
    {
        await _factory.ResetAndSeedDataAsync();

        Guid courseId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            var course = await RepositoryTestDataHelper.CreateCourseAsync(db);
            courseId = course.Id;
            _ = await RepositoryTestDataHelper.CreateCourseEventAsync(db, courseId: courseId);
        }

        using var client = _factory.CreateClient();
        var response = await client.DeleteAsync($"/api/courses/{courseId}");
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.Conflict, payload.ErrorType);
    }

    [Fact]
    public async Task CreateCourse_ThenGetById_ReturnsCreatedCourse()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var createRequest = new CreateCourseRequest
        {
            Title = "E2E Course",
            Description = "E2E Description",
            DurationInDays = 5
        };
        var createResponse = await client.PostAsJsonAsync("/api/courses", createRequest);
        var createPayload = await createResponse.Content.ReadFromJsonAsync<Result<Course>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createPayload);
        Assert.True(createPayload.Success);
        Assert.NotNull(createPayload.Value);

        var courseId = createPayload.Value.Id;
        var getResponse = await client.GetAsync($"/api/courses/{courseId}");
        var getPayload = await getResponse.Content.ReadFromJsonAsync<Result<CourseWithEvents>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(getPayload);
        Assert.True(getPayload.Success);
        Assert.NotNull(getPayload.Value);
        Assert.Equal(courseId, getPayload.Value.Course.Id);
        Assert.Equal("E2E Course", getPayload.Value.Course.Title);
        Assert.Equal("E2E Description", getPayload.Value.Course.Description);
        Assert.Equal(5, getPayload.Value.Course.DurationInDays);
    }

    [Fact]
    public async Task UpdateCourse_ReturnsOk_AndPersistsChanges()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var createRequest = new CreateCourseRequest
        {
            Title = "Initial Course",
            Description = "Initial Description",
            DurationInDays = 3
        };
        var createResponse = await client.PostAsJsonAsync("/api/courses", createRequest);
        var createPayload = await createResponse.Content.ReadFromJsonAsync<Result<Course>>(_jsonOptions);
        Assert.NotNull(createPayload?.Value);

        var courseId = createPayload.Value.Id;
        var updateRequest = new UpdateCourseRequest
        {
            Title = "Updated Course",
            Description = "Updated Description",
            DurationInDays = 10
        };
        var updateResponse = await client.PutAsJsonAsync($"/api/courses/{courseId}", updateRequest);
        var updatePayload = await updateResponse.Content.ReadFromJsonAsync<Result<Course>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updatePayload);
        Assert.True(updatePayload.Success);
        Assert.NotNull(updatePayload.Value);
        Assert.Equal("Updated Course", updatePayload.Value.Title);
        Assert.Equal("Updated Description", updatePayload.Value.Description);
        Assert.Equal(10, updatePayload.Value.DurationInDays);
    }

    [Fact]
    public async Task DeleteCourse_ReturnsOk_AndRemovesCourseFromDatabase()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var createRequest = new CreateCourseRequest
        {
            Title = "Delete Me",
            Description = "Delete Me Description",
            DurationInDays = 2
        };
        var createResponse = await client.PostAsJsonAsync("/api/courses", createRequest);
        var createPayload = await createResponse.Content.ReadFromJsonAsync<Result<Course>>(_jsonOptions);
        Assert.NotNull(createPayload?.Value);

        var courseId = createPayload.Value.Id;
        var deleteResponse = await client.DeleteAsync($"/api/courses/{courseId}");
        var deletePayload = await deleteResponse.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.NotNull(deletePayload);
        Assert.True(deletePayload.Success);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
        var existing = await db.Courses.FindAsync(courseId);
        Assert.Null(existing);
    }
}

