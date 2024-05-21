using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;

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

        apiKeysGroup.MapPost("/create", async (
            [FromForm] CreateApiKeyRequest request,
            IValidator<CreateApiKeyRequest> validator,
            DatabaseContext db,
            CancellationToken cancellationToken,
            HttpContext context) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsValid == false)
                return Results.BadRequest();

            var entity = new ApiKey
            {
                SubDomain = request.Subdomain
            };
            
            //TODO: add validation that this doesn't exist and add unique index to DB
            await db.Set<ApiKey>().AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            
            context.Response.Headers.Append("HX-Redirect", "/dashboard");
            return Results.Empty;
        }).DisableAntiforgery();

        apiKeysGroup.MapDelete("/delete/{id}", () => TypedResults.Empty);
    }
}