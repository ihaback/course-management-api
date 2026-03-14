using Backend.Infrastructure.Persistence.EFC.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Integration;

[Collection(SqliteInMemoryCollection.Name)]
public sealed class IntegrationDatabaseConfiguration_Tests(SqliteInMemoryFixture fixture)
{
    private readonly SqliteInMemoryFixture _fixture = fixture;

    [Fact]
    public void Integration_Fixture_Uses_Sqlite_InMemory_Database()
    {
        using var db = _fixture.CreateDbContext();

        Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", db.Database.ProviderName);

        var sqliteConnection = Assert.IsType<SqliteConnection>(db.Database.GetDbConnection());
        Assert.Contains(":memory:", sqliteConnection.ConnectionString, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Cache=Shared", sqliteConnection.ConnectionString, StringComparison.OrdinalIgnoreCase);
    }
}

