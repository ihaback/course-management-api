using Backend.Infrastructure.Persistence.EFC.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Integration;

public sealed class SqliteInMemoryFixture : IAsyncLifetime
{
    private SqliteConnection? _conn;

    public DbContextOptions<CoursesOnlineDbContext> Options { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        _conn = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        await _conn.OpenAsync();

        Options = new DbContextOptionsBuilder<CoursesOnlineDbContext>()
            .UseSqlite(_conn)
            .EnableSensitiveDataLogging()
            .Options;

        await using var db = new CoursesOnlineDbContext(Options);
        await db.Database.EnsureCreatedAsync();
        await SeedLookupDataAsync(db);
    }

    public async Task DisposeAsync()
    {
        if (_conn is not null)
        {
            await _conn.DisposeAsync();
        }
    }

    public CoursesOnlineDbContext CreateDbContext() => new(Options);

    private static async Task SeedLookupDataAsync(CoursesOnlineDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            INSERT OR IGNORE INTO PaymentMethods (Id, Name) VALUES
            (1, 'Card'),
            (2, 'Invoice'),
            (3, 'Cash');
            """);

        await db.Database.ExecuteSqlRawAsync("""
            INSERT OR IGNORE INTO ParticipantContactTypes (Id, Name) VALUES
            (1, 'Primary'),
            (2, 'Billing'),
            (3, 'Emergency');
            """);

        await db.Database.ExecuteSqlRawAsync("""
            INSERT OR IGNORE INTO VenueTypes (Id, Name) VALUES
            (1, 'InPerson'),
            (2, 'Online'),
            (3, 'Hybrid');
            """);

        await db.Database.ExecuteSqlRawAsync("""
            INSERT OR IGNORE INTO CourseRegistrationStatuses (Id, Name) VALUES
            (0, 'Pending'),
            (1, 'Paid'),
            (2, 'Cancelled'),
            (3, 'Refunded');
            """);
    }
}

[CollectionDefinition(Name)]
public sealed class SqliteInMemoryCollection : ICollectionFixture<SqliteInMemoryFixture>
{
    public const string Name = "SqliteInMemory";
}

