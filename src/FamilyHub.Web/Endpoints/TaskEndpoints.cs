using FamilyHub.Application.Features.Tasks;
using Mediator;

namespace FamilyHub.Web.Endpoints;

/// <summary>
/// Endpoints API REST pour les taches.
/// Demontre la gestion d'erreurs avec GlobalExceptionHandler.
/// Les erreurs metier retournent des Result, les erreurs systeme des ProblemDetails.
/// </summary>
public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks");

        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetTasks());
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(statusCode: 500);
        });

        group.MapPost("/", async (CreateTask command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/tasks/{result.Value}", result.Value)
                : Results.BadRequest(result.ValidationErrors);
        });

        group.MapPut("/{id:guid}/complete", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new CompleteTask(id));
            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.ValidationErrors);
        });

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteTask(id));
            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound();
        });
    }
}
