namespace Microsoft.AspNetCore.Builder;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health")
            .WithTags("Health")
            .AllowAnonymous();

        app.MapGet("/", () => Results.Redirect("/scalar/v1"))
            .ExcludeFromDescription();
    }
}
