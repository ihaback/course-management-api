using Backend.Infrastructure.Persistence.Entities;
using Backend.Infrastructure.Persistence.EFC.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.EFC.Context;

public sealed class CoursesOnlineDbContext(DbContextOptions<CoursesOnlineDbContext> options) : DbContext(options)
{
    public DbSet<CourseEntity> Courses => Set<CourseEntity>();
    public DbSet<CourseEventEntity> CourseEvents => Set<CourseEventEntity>();
    public DbSet<CourseEventTypeEntity> CourseEventTypes => Set<CourseEventTypeEntity>();
    public DbSet<InstructorEntity> Instructors => Set<InstructorEntity>();
    public DbSet<InstructorRoleEntity> InstructorRoles => Set<InstructorRoleEntity>();
    public DbSet<LocationEntity> Locations => Set<LocationEntity>();
    public DbSet<ParticipantEntity> Participants => Set<ParticipantEntity>();
    public DbSet<InPlaceLocationEntity> InPlaceLocations => Set<InPlaceLocationEntity>();
    public DbSet<CourseRegistrationEntity> CourseRegistrations => Set<CourseRegistrationEntity>();
    public DbSet<CourseRegistrationStatusEntity> CourseRegistrationStatuses => Set<CourseRegistrationStatusEntity>();
    public DbSet<PaymentMethodEntity> PaymentMethods => Set<PaymentMethodEntity>();
    public DbSet<ParticipantContactTypeEntity> ParticipantContactTypes => Set<ParticipantContactTypeEntity>();
    public DbSet<VenueTypeEntity> VenueTypes => Set<VenueTypeEntity>();
    public DbSet<CourseEventInstructorEntity> CourseEventInstructors => Set<CourseEventInstructorEntity>();
    public DbSet<InPlaceEventLocationEntity> InPlaceEventLocations => Set<InPlaceEventLocationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var isSqlite = Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";

        modelBuilder.ApplyConfiguration(new CourseEntityConfiguration(isSqlite));
        modelBuilder.ApplyConfiguration(new CourseEventEntityConfiguration(isSqlite));
        modelBuilder.ApplyConfiguration(new CourseEventTypeEntityConfiguration(isSqlite));
        modelBuilder.ApplyConfiguration(new CourseRegistrationEntityConfiguration(isSqlite));
        modelBuilder.ApplyConfiguration(new CourseRegistrationStatusEntityConfiguration(isSqlite));
        modelBuilder.ApplyConfiguration(new InPlaceLocationEntityConfiguration(isSqlite));
        modelBuilder.ApplyConfiguration(new InstructorEntityConfiguration(isSqlite));
        modelBuilder.ApplyConfiguration(new InstructorRoleEntityConfiguration(isSqlite));
        modelBuilder.ApplyConfiguration(new LocationEntityConfiguration(isSqlite));
        modelBuilder.ApplyConfiguration(new ParticipantContactTypeEntityConfiguration(isSqlite));
        modelBuilder.ApplyConfiguration(new ParticipantEntityConfiguration(isSqlite));
        modelBuilder.ApplyConfiguration(new PaymentMethodEntityConfiguration(isSqlite));
        modelBuilder.ApplyConfiguration(new VenueTypeEntityConfiguration(isSqlite));
    }
}

