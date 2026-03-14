using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Application.Modules.Participants.Outputs;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Application.Common;
using Backend.Presentation.API.Models.Participant;
using Backend.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Backend.Domain.Modules.Participants.Models;

namespace Backend.Tests.E2E.Participants;

public sealed class ParticipantsEndpoints_Tests(CoursesOnlineDbApiFactory factory) : IClassFixture<CoursesOnlineDbApiFactory>
{
    private readonly CoursesOnlineDbApiFactory _factory = factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetAllParticipants_ReturnsOk_WithEmptyList_AfterReset()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/participants");
        var payload = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<Participant>>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload.Success);
        Assert.NotNull(payload.Value);
        Assert.Empty(payload.Value);
    }

    [Fact]
    public async Task GetAllParticipants_ReturnsNewestFirst_ByCreatedAtDescending()
    {
        await _factory.ResetAndSeedDataAsync();

        int contactTypeId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            contactTypeId = await db.ParticipantContactTypes.AsNoTracking().Select(x => x.Id).FirstAsync();
        }

        using var client = _factory.CreateClient();
        var firstCreate = await client.PostAsJsonAsync("/api/participants", new CreateParticipantRequest
        {
            FirstName = "First",
            LastName = "Order",
            Email = $"first-order-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "111111111",
            ContactTypeId = contactTypeId
        });
        var firstId = Guid.Parse(firstCreate.Headers.Location!.OriginalString.Split('/')[^1]);

        var secondCreate = await client.PostAsJsonAsync("/api/participants", new CreateParticipantRequest
        {
            FirstName = "Second",
            LastName = "Order",
            Email = $"second-order-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "222222222",
            ContactTypeId = contactTypeId
        });
        var secondId = Guid.Parse(secondCreate.Headers.Location!.OriginalString.Split('/')[^1]);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            var firstEntity = await db.Participants.FindAsync(firstId);
            var secondEntity = await db.Participants.FindAsync(secondId);
            Assert.NotNull(firstEntity);
            Assert.NotNull(secondEntity);
            firstEntity!.CreatedAtUtc = DateTime.UtcNow.AddMinutes(-2);
            secondEntity!.CreatedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/participants");
        var payload = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<Participant>>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.NotNull(payload.Value);

        var firstIndex = payload.Value.ToList().FindIndex(x => x.Id == firstId);
        var secondIndex = payload.Value.ToList().FindIndex(x => x.Id == secondId);

        Assert.True(firstIndex >= 0);
        Assert.True(secondIndex >= 0);
        Assert.True(secondIndex < firstIndex);
    }

    [Fact]
    public async Task CreateParticipant_ThenGetById_ReturnsCreatedParticipant()
    {
        await _factory.ResetAndSeedDataAsync();

        int contactTypeId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            contactTypeId = await db.ParticipantContactTypes.AsNoTracking().Select(x => x.Id).FirstAsync();
        }

        using var client = _factory.CreateClient();
        var createRequest = new CreateParticipantRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = $"ada-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "123456789",
            ContactTypeId = contactTypeId
        };

        var createResponse = await client.PostAsJsonAsync("/api/participants", createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var createdParticipantId = Guid.Parse(createResponse.Headers.Location!.OriginalString.Split('/')[^1]);
        var getResponse = await client.GetAsync($"/api/participants/{createdParticipantId}");
        var getPayload = await getResponse.Content.ReadFromJsonAsync<Result<ParticipantDetails>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(getPayload);
        Assert.True(getPayload.Success);
        Assert.NotNull(getPayload.Value);
        Assert.Equal(createdParticipantId, getPayload.Value.Id);
        Assert.Equal("Ada", getPayload.Value.FirstName);
        Assert.Equal("Lovelace", getPayload.Value.LastName);
        Assert.Equal(contactTypeId, getPayload.Value.ContactType.Id);
    }

    [Fact]
    public async Task CreateParticipant_ReturnsNotFound_WhenContactTypeDoesNotExist()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var createRequest = new CreateParticipantRequest
        {
            FirstName = "Grace",
            LastName = "Hopper",
            Email = $"grace-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "987654321",
            ContactTypeId = int.MaxValue
        };

        var response = await client.PostAsJsonAsync("/api/participants", createRequest);
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.NotFound, payload.ErrorType);
    }

    [Fact]
    public async Task GetParticipantById_ReturnsBadRequest_ForEmptyGuid()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/participants/{Guid.Empty}");
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.BadRequest, payload.ErrorType);
    }

    [Fact]
    public async Task DeleteParticipant_ReturnsConflict_WhenParticipantHasRegistrations()
    {
        await _factory.ResetAndSeedDataAsync();

        Guid participantId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            var participant = await RepositoryTestDataHelper.CreateParticipantAsync(db);
            var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(db, seats: 10);
            _ = await RepositoryTestDataHelper.CreateCourseRegistrationAsync(
                db,
                participantId: participant.Id,
                courseEventId: courseEvent.Id);
            participantId = participant.Id;
        }

        using var client = _factory.CreateClient();
        var response = await client.DeleteAsync($"/api/participants/{participantId}");
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.Conflict, payload.ErrorType);    }

    [Fact]
    public async Task DeleteParticipant_ReturnsOk_AndRemovesParticipant()
    {
        await _factory.ResetAndSeedDataAsync();

        Guid participantId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            participantId = (await RepositoryTestDataHelper.CreateParticipantAsync(db)).Id;
        }

        using var client = _factory.CreateClient();

        var deleteResponse = await client.DeleteAsync($"/api/participants/{participantId}");
        var deletePayload = await deleteResponse.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.NotNull(deletePayload);
        Assert.True(deletePayload.Success);
        var getResponse = await client.GetAsync($"/api/participants/{participantId}");
        var getPayload = await getResponse.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.NotNull(getPayload);
        Assert.False(getPayload.Success);
        Assert.Equal(ErrorTypes.NotFound, getPayload.ErrorType);
    }
}
