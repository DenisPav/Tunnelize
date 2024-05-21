using FluentValidation;
using Microsoft.AspNetCore.Mvc;

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

        apiKeysGroup.MapPost("/create", (
            [FromForm] CreateApiKeyRequest request,
            IValidator<CreateApiKeyRequest> validator,
            HttpContext context) =>
        {
            var validationResult = validator.Validate(request);
            if (validationResult.IsValid == false)
                return Results.BadRequest();
            
            context.Response.Headers.Append("HX-Redirect", "/dashboard");
            return Results.Empty;
        }).DisableAntiforgery();

        apiKeysGroup.MapDelete("/delete/{id}", () => { return TypedResults.Empty; });
    }
}