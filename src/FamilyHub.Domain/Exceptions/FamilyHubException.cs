namespace FamilyHub.Domain.Exceptions;

/// <summary>
/// Exception de base pour FamilyHub.
/// Toutes les exceptions metier heritent de cette classe.
/// </summary>
public class FamilyHubException : Exception
{
    public FamilyHubException(string message) : base(message) { }
    public FamilyHubException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// L'entite demandee n'existe pas.
/// SECURITE: Le message ne contient PAS le type d'entite ni l'ID en production.
/// </summary>
public class EntityNotFoundException : FamilyHubException
{
    public EntityNotFoundException(string entityName, Guid id)
        : base($"Entity '{entityName}' with id '{id}' was not found.") { }
}

/// <summary>
/// Violation d'une regle metier.
/// </summary>
public class BusinessRuleException : FamilyHubException
{
    public string RuleCode { get; }
    public BusinessRuleException(string ruleCode, string message)
        : base(message) { RuleCode = ruleCode; }
}
