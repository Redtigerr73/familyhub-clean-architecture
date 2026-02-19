using FamilyHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Infrastructure.Database.Configurations;

/// <summary>
/// Configuration EF Core pour l'entite FamilyTask.
/// </summary>
public class FamilyTaskConfiguration : IEntityTypeConfiguration<FamilyTask>
{
    public void Configure(EntityTypeBuilder<FamilyTask> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        // Stocke l'enum comme string en base pour la lisibilite
        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}
