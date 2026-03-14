using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.EFC.Configurations;

public sealed class LocationEntityConfiguration(bool isSqlite) : IEntityTypeConfiguration<LocationEntity>
{
    public void Configure(EntityTypeBuilder<LocationEntity> e)
    {

        e.ToTable("Locations", t =>
        {
            t.HasCheckConstraint("CK_Locations_PostalCode_NotEmpty", "LTRIM(RTRIM([PostalCode])) <> ''");
        });

        e.HasKey(x => x.Id).HasName("PK_Locations_Id");

        e.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        e.Property(x => x.StreetName)
            .HasMaxLength(50)
            .IsRequired();

        e.Property(x => x.PostalCode)
            .HasMaxLength(6)
            .IsUnicode(false)
            .IsRequired();

        e.Property(x => x.City)
            .HasMaxLength(50)
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

        e.HasIndex(x => x.PostalCode)
            .HasDatabaseName("IX_Locations_PostalCode");
    }
}

