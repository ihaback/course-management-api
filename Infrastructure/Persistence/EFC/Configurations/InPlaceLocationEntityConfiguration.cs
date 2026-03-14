using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.EFC.Configurations;

public sealed class InPlaceLocationEntityConfiguration(bool isSqlite) : IEntityTypeConfiguration<InPlaceLocationEntity>
{
    public void Configure(EntityTypeBuilder<InPlaceLocationEntity> e)
    {

        e.ToTable("InPlaceLocations");

        e.HasKey(x => x.Id).HasName("PK_InPlaceLocations_Id");

        e.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        e.Property(x => x.RoomNumber)
            .IsRequired();

        e.Property(x => x.Seats)
            .IsRequired();

        if (isSqlite)
        {
            e.Property(x => x.Concurrency)
                .IsConcurrencyToken()
                .IsRequired(false);
        }
        else
        {
            e.Property(x => x.Concurrency)
                .IsRowVersion()
                .IsConcurrencyToken()
                .IsRequired();
        }

        e.HasOne(ipl => ipl.Location)
            .WithMany(l => l.InPlaceLocations)
            .HasForeignKey(ipl => ipl.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(x => new { x.LocationId, x.RoomNumber })
            .IsUnique()
            .HasDatabaseName("IX_InPlaceLocations_LocationId_RoomNumber");
    }
}

