using FamilyHub.Domain.Entities;

namespace FamilyHub.Domain.Interfaces;

/// <summary>
/// Interface du repository pour les taches familiales.
/// Definie dans le DOMAIN mais IMPLEMENTEE dans Infrastructure.
/// C'est le principe d'inversion de dependance (DIP) en action !
///
/// Le Domain dit "j'ai besoin de CA" (l'interface),
/// et l'Infrastructure repond "voici COMMENT je le fais" (l'implementation).
/// </summary>
public interface IFamilyTaskRepository
{
    Task<FamilyTask?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<FamilyTask>> GetAllAsync();
    Task<IReadOnlyList<FamilyTask>> GetByMemberIdAsync(Guid memberId);
    Task AddAsync(FamilyTask task);
    Task UpdateAsync(FamilyTask task);
    Task DeleteAsync(Guid id);
}
