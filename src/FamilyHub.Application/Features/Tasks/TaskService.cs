using FamilyHub.Application.Interfaces;
using FamilyHub.Domain.Entities;
using FamilyHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Application.Features.Tasks;

/// <summary>
/// Service applicatif pour la gestion des taches familiales.
/// Orchestre les cas d'utilisation lies aux taches.
/// </summary>
public class TaskService(IFamilyHubDbContext context)
{
    public async Task<IReadOnlyList<FamilyTask>> GetAllTasksAsync()
    {
        return await context.Tasks
            .AsNoTracking()
            .Include(t => t.AssignedTo) // Inclut le membre assigne
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<FamilyTask?> GetTaskByIdAsync(Guid id)
    {
        return await context.Tasks
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<FamilyTask> CreateTaskAsync(
        string title,
        string? description,
        TaskPriority priority,
        DateTime? dueDate,
        Guid? assignedToId)
    {
        var task = new FamilyTask
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Priority = priority,
            DueDate = dueDate,
            AssignedToId = assignedToId
        };

        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        return task;
    }

    public async Task<bool> CompleteTaskAsync(Guid id)
    {
        var task = await context.Tasks.FindAsync(id);
        if (task is null)
            return false;

        // La logique metier est dans l'entite FamilyTask.Complete()
        // Le service ne fait que coordonner
        var success = task.Complete();
        if (!success)
            return false;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTaskAsync(Guid id)
    {
        var task = await context.Tasks.FindAsync(id);
        if (task is null)
            return false;

        context.Tasks.Remove(task);
        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>Recupere les taches en retard</summary>
    public async Task<IReadOnlyList<FamilyTask>> GetOverdueTasksAsync()
    {
        var tasks = await context.Tasks
            .AsNoTracking()
            .Include(t => t.AssignedTo)
            .Where(t => t.DueDate != null
                && t.DueDate < DateTime.UtcNow
                && t.Status != FamilyTaskStatus.Done)
            .ToListAsync();

        return tasks;
    }
}
