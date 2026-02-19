namespace FamilyHub.Domain.Entities;

/// <summary>
/// Represente un article de la liste de courses familiale.
/// </summary>
public class ShoppingItem
{
    public Guid Id { get; set; }

    /// <summary>Nom de l'article (ex: "Lait", "Pain")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Quantite souhaitee</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Est-ce que l'article a ete achete ?</summary>
    public bool IsPurchased { get; set; }

    /// <summary>Categorie (Fruits, Viandes, Hygiene...)</summary>
    public string? Category { get; set; }

    /// <summary>Date d'ajout a la liste</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Qui a ajoute cet article</summary>
    public Guid? AddedById { get; set; }
    public FamilyMember? AddedBy { get; set; }

    /// <summary>Marque l'article comme achete</summary>
    public void MarkAsPurchased()
    {
        IsPurchased = true;
    }
}
