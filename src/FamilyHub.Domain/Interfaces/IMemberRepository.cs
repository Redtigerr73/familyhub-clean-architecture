using FamilyHub.Domain.Entities;

namespace FamilyHub.Domain.Interfaces;

/// <summary>
/// Interface du repository pour les membres de la famille.
/// Meme principe que IFamilyTaskRepository : defini ici, implemente ailleurs.
/// </summary>
public interface IMemberRepository
{
    Task<FamilyMember?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<FamilyMember>> GetAllAsync();
    Task AddAsync(FamilyMember member);
    Task UpdateAsync(FamilyMember member);
    Task DeleteAsync(Guid id);
}
