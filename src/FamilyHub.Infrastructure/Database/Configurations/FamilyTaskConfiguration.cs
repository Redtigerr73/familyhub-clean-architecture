using FamilyHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Infrastructure.Database.Configurations;

/// <summary>
/// Configuration EF Core pour l'entite FamilyTask.
///
/// Pragmatic Architecture : Mise a jour pour supporter :
/// - AuditInfo comme Owned Entity (colonnes integrees dans la table Tasks)
/// - DomainEvents ignore (propriete transitoire, pas stockee en base)
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

        // Pragmatic Architecture : AuditInfo est un Owned Entity (Value Object DDD)
        // Ses proprietes seront des colonnes dans la table Tasks :
        //   Audit_Created, Audit_CreatedBy, Audit_Modified, Audit_ModifiedBy
        builder.OwnsOne(t => t.Audit);

        // Pragmatic Architecture : Ignorer DomainEvents (propriete transitoire)
        // Les evenements de domaine ne doivent PAS etre stockes en base
        builder.Ignore(t => t.DomainEvents);
    }
}
