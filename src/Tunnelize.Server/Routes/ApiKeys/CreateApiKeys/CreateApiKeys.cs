using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Tunnelize.Server.Components.ApiKeys;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;

namespace Tunnelize.Server.Routes.ApiKeys.CreateApiKeys;

public class CreateApiKeys : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api-keys/create", Handle).DisableAntiforgery();
    }

    private static async Task<IResult> Handle(
        [FromForm] CreateApiKeyRequest request,
        IValidator<CreateApiKeyRequest> validator,
        DatabaseContext db,
        CancellationToken cancellationToken,
        HttpContext context)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid == false)
        {
            return new RazorComponentResult<Create>(new { HasErrors = true });    
        }

        var entity = new ApiKeyMapper().MapFromRequest(request);
        entity.UserId = context.GetUserId();

        await db.Set<ApiKey>().AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        
        context.Response.Headers.Append("HX-Redirect", "/dashboard");
        return Results.Empty;
    }
}