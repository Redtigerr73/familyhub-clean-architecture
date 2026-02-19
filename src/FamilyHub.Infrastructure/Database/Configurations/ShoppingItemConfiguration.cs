using FamilyHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyHub.Infrastructure.Database.Configurations;

/// <summary>
/// Configuration EF Core pour l'entite ShoppingItem.
/// </summary>
public class ShoppingItemConfiguration : IEntityTypeConfiguration<ShoppingItem>
{
    public void Configure(EntityTypeBuilder<ShoppingItem> builder)
    {
        builder.ToTable("ShoppingItems");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Category)
            .HasMaxLength(100);

        builder.Property(s => s.Quantity)
            .HasDefaultValue(1);

        // Relation : un article est ajoute par un membre
        builder.HasOne(s => s.AddedBy)
            .WithMany()
            .HasForeignKey(s => s.AddedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
