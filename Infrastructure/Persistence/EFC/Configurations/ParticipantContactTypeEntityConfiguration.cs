using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.EFC.Configurations;

public sealed class ParticipantContactTypeEntityConfiguration(bool isSqlite) : IEntityTypeConfiguration<ParticipantContactTypeEntity>
{
    public void Configure(EntityTypeBuilder<ParticipantContactTypeEntity> e)
    {

        e.ToTable("ParticipantContactTypes");

        e.HasKey(x => x.Id).HasName("PK_ParticipantContactTypes_Id");

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
            .HasDatabaseName("IX_ParticipantContactTypes_Name");

    }
}
