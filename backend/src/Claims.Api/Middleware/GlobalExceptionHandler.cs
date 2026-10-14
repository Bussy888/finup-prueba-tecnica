using Claims.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem;

        switch (exception)
        {
            case ValidationFailedException validation:
                problem = new ValidationProblemDetails(validation.Errors)
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Los datos enviados no son válidos."
                };
                break;
            case NotFoundException notFound:
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "No encontrado",
                    Detail = notFound.Message
                };
                break;
            case ConflictException conflict:
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflicto",
                    Detail = conflict.Message
                };
                break;
            default:
                logger.LogError(exception, "Error no controlado en {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Error interno",
                    Detail = "Ocurrió un error inesperado. Intenta de nuevo más tarde."
                };
                break;
        }

        problem.Instance = context.Request.Path;
        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, problem.GetType(), cancellationToken);
        return true;
    }
}
