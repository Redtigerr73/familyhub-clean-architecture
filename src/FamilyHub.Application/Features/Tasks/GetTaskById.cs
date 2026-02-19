using Ardalis.Result;
using FamilyHub.Application.Interfaces;
using FamilyHub.Domain.Enums;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Application.Features.Tasks;

// =============================================================================
// CQRS: GetTaskById - Requete pour recuperer une tache par son identifiant
//
// Illustre une query avec un parametre obligatoire (l'Id).
// Retourne un Result<TaskDetailDto> pour gerer le cas "introuvable".
// =============================================================================

/// <summary>
/// CQRS: Requete pour recuperer une tache par son Id.
/// </summary>
public record GetTaskById(Guid TaskId) : IQuery<Result<TaskDetailDto>>;

/// <summary>
/// CQRS: Validateur pour GetTaskById.
/// Meme les requetes peuvent avoir des validateurs !
/// </summary>
public class GetTaskByIdValidator : AbstractValidator<GetTaskById>
{
    public GetTaskByIdValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("L'identifiant de la tache est obligatoire.");
    }
}

/// <summary>
/// CQRS: Handler de la requete GetTaskById.
/// </summary>
public class GetTaskByIdHandler(IFamilyHubDbContext context)
    : IQueryHandler<GetTaskById, Result<TaskDetailDto>>
{
    public async ValueTask<Result<TaskDetailDto>> Handle(GetTaskById query, CancellationToken ct)
    {
        var task = await context.Tasks
            .AsNoTracking()
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == query.TaskId, ct);

        if (task is null)
            return Result.NotFound($"Tache {query.TaskId} introuvable.");

        var dto = new TaskDetailDto(
            task.Id,
            task.Title,
            task.Description,
            task.Priority,
            task.Status,
            task.DueDate,
            task.CreatedAt,
            task.CompletedAt,
            task.AssignedToId,
            task.AssignedTo?.FullName,
            task.IsOverdue
        );

        return Result.Success(dto);
    }
}
