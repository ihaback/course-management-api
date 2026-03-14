using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Application.Common;
using Backend.Presentation.API.Models.Location;
using Backend.Tests.Integration.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Backend.Domain.Modules.Locations.Models;

namespace Backend.Tests.E2E.Locations;

public sealed class LocationsEndpoints_Tests(CoursesOnlineDbApiFactory factory) : IClassFixture<CoursesOnlineDbApiFactory>
{
    private readonly CoursesOnlineDbApiFactory _factory = factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetAllLocations_ReturnsOk_WithEmptyList_AfterReset()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/locations");
        var payload = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<Location>>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload.Success);
        Assert.NotNull(payload.Value);
        Assert.Empty(payload.Value);
    }

    [Fact]
    public async Task GetAllLocations_ReturnsNewestFirst_ByIdDescending()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var firstCreate = await client.PostAsJsonAsync("/api/locations", new CreateLocationRequest
        {
            StreetName = "Order Street 1",
            PostalCode = "10001",
            City = "CityA"
        });
        var firstId = int.Parse(firstCreate.Headers.Location!.OriginalString.Split('/')[^1]);

        var secondCreate = await client.PostAsJsonAsync("/api/locations", new CreateLocationRequest
        {
            StreetName = "Order Street 2",
            PostalCode = "10002",
            City = "CityB"
        });
        var secondId = int.Parse(secondCreate.Headers.Location!.OriginalString.Split('/')[^1]);

        var response = await client.GetAsync("/api/locations");
        var payload = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<Location>>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload?.Value);
        Assert.Equal(secondId, payload.Value[0].Id);
        Assert.Equal(firstId, payload.Value[1].Id);
    }

    [Fact]
    public async Task CreateLocation_ThenGetById_ReturnsCreatedLocation()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var createRequest = new CreateLocationRequest
        {
            StreetName = "Main Street 1",
            PostalCode = "12345",
            City = "Stockholm"
        };

        var createResponse = await client.PostAsJsonAsync("/api/locations", createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var createdLocationId = int.Parse(createResponse.Headers.Location!.OriginalString.Split('/')[^1]);
        var getResponse = await client.GetAsync($"/api/locations/{createdLocationId}");
        var getPayload = await getResponse.Content.ReadFromJsonAsync<Result<Location>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(getPayload);
        Assert.True(getPayload.Success);
        Assert.NotNull(getPayload.Value);
        Assert.Equal(createdLocationId, getPayload.Value.Id);
        Assert.Equal("Main Street 1", getPayload.Value.StreetName);
        Assert.Equal("12345", getPayload.Value.PostalCode);
        Assert.Equal("Stockholm", getPayload.Value.City);
    }

    [Fact]
    public async Task CreateLocation_ReturnsBadRequest_ForInvalidPayload()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        using var content = new StringContent("{\"streetName\":123}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/locations", content);
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
    }

    [Fact]
    public async Task UpdateLocation_ReturnsNotFound_WhenLocationDoesNotExist()
    {
        await _factory.ResetAndSeedDataAsync();
        using var client = _factory.CreateClient();

        var request = new UpdateLocationRequest
        {
            StreetName = "Unknown Street 99",
            PostalCode = "54321",
            City = "Uppsala"
        };

        var response = await client.PutAsJsonAsync("/api/locations/999999", request);
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.NotFound, payload.ErrorType);
    }

    [Fact]
    public async Task DeleteLocation_ReturnsConflict_WhenLocationHasInPlaceLocations()
    {
        await _factory.ResetAndSeedDataAsync();

        int locationId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            var location = await RepositoryTestDataHelper.CreateLocationAsync(db);
            _ = await RepositoryTestDataHelper.CreateInPlaceLocationAsync(db, location.Id);
            locationId = location.Id;
        }

        using var client = _factory.CreateClient();
        var response = await client.DeleteAsync($"/api/locations/{locationId}");
        var payload = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal(ErrorTypes.Conflict, payload.ErrorType);    }

    [Fact]
    public async Task DeleteLocation_ReturnsOk_AndRemovesLocation()
    {
        await _factory.ResetAndSeedDataAsync();

        int locationId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();
            locationId = (await RepositoryTestDataHelper.CreateLocationAsync(db)).Id;
        }

        using var client = _factory.CreateClient();

        var deleteResponse = await client.DeleteAsync($"/api/locations/{locationId}");
        var deletePayload = await deleteResponse.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.NotNull(deletePayload);
        Assert.True(deletePayload.Success);
        var getResponse = await client.GetAsync($"/api/locations/{locationId}");
        var getPayload = await getResponse.Content.ReadFromJsonAsync<Result>(_jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.NotNull(getPayload);
        Assert.False(getPayload.Success);
        Assert.Equal(ErrorTypes.NotFound, getPayload.ErrorType);
    }
}
