using Ardalis.Result;
using FamilyHub.Application.Interfaces;
using FluentValidation;
using Mediator;

namespace FamilyHub.Application.Features.Members;

// =============================================================================
// CQRS: DeleteMember - Commande pour supprimer un membre de la famille
// =============================================================================

/// <summary>
/// CQRS: Commande pour supprimer un membre.
/// </summary>
public record DeleteMember(Guid MemberId) : ICommand<Result>;

/// <summary>
/// CQRS: Validateur pour DeleteMember.
/// </summary>
public class DeleteMemberValidator : AbstractValidator<DeleteMember>
{
    public DeleteMemberValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty().WithMessage("L'identifiant du membre est obligatoire.");
    }
}

/// <summary>
/// CQRS: Handler de la commande DeleteMember.
///
/// Pragmatic Architecture : SaveChangesAsync est retire du handler.
/// Le UnitOfWorkBehavior s'en charge automatiquement apres chaque commande.
/// </summary>
public class DeleteMemberHandler(IFamilyHubDbContext context)
    : ICommandHandler<DeleteMember, Result>
{
    public async ValueTask<Result> Handle(DeleteMember command, CancellationToken ct)
    {
        var member = await context.Members.FindAsync([command.MemberId], ct);

        if (member is null)
            return Result.NotFound($"Membre {command.MemberId} introuvable.");

        context.Members.Remove(member);

        // Pragmatic Architecture : pas de SaveChangesAsync ici
        // Le UnitOfWorkBehavior s'en charge automatiquement
        return Result.Success();
    }
}
