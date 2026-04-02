using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Builder;

public static class ErrorEndpoints
{
    public static void MapErrorEndpoint(this WebApplication app)
    {
        app.Map("/error", (HttpContext context) =>
        {
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

            var problem = new ProblemDetails
            {
                Title = "An unexpected error occurred",
                Status = StatusCodes.Status500InternalServerError,
                Detail = app.Environment.IsDevelopment() ? exception?.Message : null,
            };

            return Results.Problem(problem);
        })
        .ExcludeFromDescription();
    }
}
