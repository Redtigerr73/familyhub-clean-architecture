using FamilyHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Infrastructure.Database.Configurations;

/// <summary>
/// Configuration EF Core pour l'entite FamilyMember.
/// Separer la configuration du DbContext garde le code propre et maintenable.
/// </summary>
public class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
    public void Configure(EntityTypeBuilder<FamilyMember> builder)
    {
        builder.ToTable("Members");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Email)
            .HasMaxLength(255);

        builder.Property(m => m.Role)
            .IsRequired()
            .HasMaxLength(50);

        // Relation : un membre a plusieurs taches
        builder.HasMany(m => m.AssignedTasks)
            .WithOne(t => t.AssignedTo)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull); // Si on supprime un membre, ses taches restent
    }
}
