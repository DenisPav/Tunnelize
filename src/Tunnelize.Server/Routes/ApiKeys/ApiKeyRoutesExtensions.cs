namespace Tunnelize.Server.Routes.ApiKeys;

public static class ApiKeyRoutesExtensions
{
    public static void MapApiKeyRoutes(this IEndpointRouteBuilder app)
    {
        var apiKeysGroup = app.MapGroup("api-keys");
        
        apiKeysGroup.MapGet("/create", (HttpContext context) =>
        {
            context.Response.Headers.Append("HX-Redirect", "/create");
            return TypedResults.Empty;
        });
        
        apiKeysGroup.MapPost("/create", (HttpContext context) =>
        {
            context.Response.Headers.Append("HX-Redirect", "/dashboard");
            return TypedResults.Empty;
        });
        
        apiKeysGroup.MapDelete("/delete/{id}", () =>
        {
            return TypedResults.Empty;
        });
    }
}