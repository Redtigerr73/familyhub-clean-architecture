using Ardalis.Result;
using FamilyHub.Application.Interfaces;
using FamilyHub.Domain.Entities;
using FluentValidation;
using Mediator;

namespace FamilyHub.Application.Features.Members;

// =============================================================================
// CQRS: CreateMember - Commande pour ajouter un nouveau membre a la famille
//
// Meme structure que CreateTask : Record + Validator + Handler dans un seul fichier.
// Cette organisation "vertical slice" facilite la navigation dans le code :
// tout ce qui concerne la creation d'un membre est ICI.
// =============================================================================

/// <summary>
/// CQRS: Commande pour creer un membre de la famille.
/// </summary>
public record CreateMember(
    string FirstName,
    string LastName,
    string? Email,
    string Role
) : ICommand<Result<Guid>>;

/// <summary>
/// CQRS: Validateur pour CreateMember.
/// FluentValidation permet d'ecrire les regles de validation de maniere lisible et declarative.
/// </summary>
public class CreateMemberValidator : AbstractValidator<CreateMember>
{
    public CreateMemberValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Le prenom est obligatoire.")
            .MaximumLength(100).WithMessage("Le prenom ne peut pas depasser 100 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Le nom est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom ne peut pas depasser 100 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("L'email n'est pas valide.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Le role est obligatoire.")
            .MaximumLength(50).WithMessage("Le role ne peut pas depasser 50 caracteres.");
    }
}

/// <summary>
/// CQRS: Handler de la commande CreateMember.
///
/// Pragmatic Architecture : SaveChangesAsync est retire du handler.
/// Le UnitOfWorkBehavior s'en charge automatiquement apres chaque commande.
/// </summary>
public class CreateMemberHandler(IFamilyHubDbContext context)
    : ICommandHandler<CreateMember, Result<Guid>>
{
    public ValueTask<Result<Guid>> Handle(CreateMember command, CancellationToken ct)
    {
        var member = new FamilyMember
        {
            Id = Guid.NewGuid(),
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            Role = command.Role
        };

        context.Members.Add(member);

        // Pragmatic Architecture : pas de SaveChangesAsync ici
        // Le UnitOfWorkBehavior s'en charge automatiquement
        return ValueTask.FromResult(Result.Success(member.Id));
    }
}
