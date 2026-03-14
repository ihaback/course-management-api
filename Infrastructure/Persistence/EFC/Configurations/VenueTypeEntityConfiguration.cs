using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.EFC.Configurations;

public sealed class VenueTypeEntityConfiguration(bool isSqlite) : IEntityTypeConfiguration<VenueTypeEntity>
{
    public void Configure(EntityTypeBuilder<VenueTypeEntity> e)
    {

        e.ToTable("VenueTypes");

        e.HasKey(x => x.Id).HasName("PK_VenueTypes_Id");

        e.Property(x => x.Id)
            .ValueGeneratedNever();

        e.Property(x => x.Name)
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

        e.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("IX_VenueTypes_Name");

    }
}
