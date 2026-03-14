using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Presentation.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Tests.E2E;

public sealed class CoursesOnlineDbApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    public async Task ResetAndSeedDataAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoursesOnlineDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        await SeedLookupDataAsync(db);
    }

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

