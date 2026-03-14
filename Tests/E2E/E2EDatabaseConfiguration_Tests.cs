using Backend.Infrastructure.Persistence.EFC.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Tests.E2E;

public sealed class E2EDatabaseConfiguration_Tests(CoursesOnlineDbApiFactory factory) : IClassFixture<CoursesOnlineDbApiFactory>
{
    private readonly CoursesOnlineDbApiFactory _factory = factory;

    [Fact]
    public async Task E2E_Host_Uses_Sqlite_InMemory_Database()
    {
        await _factory.ResetAndSeedDataAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();

        Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", db.Database.ProviderName);

        var sqliteConnection = Assert.IsType<SqliteConnection>(db.Database.GetDbConnection());
        Assert.Contains(":memory:", sqliteConnection.ConnectionString, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Cache=Shared", sqliteConnection.ConnectionString, StringComparison.OrdinalIgnoreCase);
    }
}

