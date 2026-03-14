using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.EFC.Configurations;

public sealed class ParticipantEntityConfiguration(bool isSqlite) : IEntityTypeConfiguration<ParticipantEntity>
{
    public void Configure(EntityTypeBuilder<ParticipantEntity> e)
    {

        e.ToTable("Participants", t =>
        {
            t.HasCheckConstraint("CK_Participants_Email_NotEmpty", "LTRIM(RTRIM([Email])) <> ''");
        });

        e.HasKey(x => x.Id).HasName("PK_Participants_Id");

        e.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("(NEWSEQUENTIALID())", "DF_Participants_Id");

        e.Property(x => x.FirstName)
            .HasMaxLength(50)
            .IsRequired();

        e.Property(x => x.LastName)
            .HasMaxLength(50)
            .IsRequired();

        e.Property(x => x.Email)
            .HasMaxLength(100)
            .IsRequired();

        e.Property(x => x.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        e.Property(x => x.ContactTypeId)
            .HasDefaultValue(1)
            .IsRequired();

        if (isSqlite)
        {
            e.Property(x => x.Concurrency)
                .IsConcurrencyToken()
                .IsRequired(false);

            e.Property(x => x.CreatedAtUtc)
                .HasPrecision(0)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            e.Property(x => x.ModifiedAtUtc)
                .HasPrecision(0)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();
        }
        else
        {
            e.Property(x => x.Concurrency)
                .IsRowVersion()
                .IsConcurrencyToken()
                .IsRequired();

            e.Property(x => x.CreatedAtUtc)
                .HasPrecision(0)
                .HasDefaultValueSql("(SYSUTCDATETIME())", "DF_Participants_CreatedAtUtc")
                .ValueGeneratedOnAdd();

            e.Property(x => x.ModifiedAtUtc)
                .HasPrecision(0)
                .HasDefaultValueSql("(SYSUTCDATETIME())", "DF_Participants_ModifiedAtUtc")
                .ValueGeneratedOnAddOrUpdate();
        }

        e.HasOne(p => p.ContactType)
            .WithMany()
            .HasForeignKey(p => p.ContactTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("IX_Participants_Email");
    }
}

