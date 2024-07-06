using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tunnelize.Client.Components.ApiKeys;
using Tunnelize.Client.Persistence;
using Tunnelize.Client.Persistence.Entities;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Client.Routes.ApiKeys.AddApiKey;

public class AddApiKey : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/api-keys", Handle);
    }

    private static async Task<IResult> Handle(
        HttpContext context,
        [FromForm] AddApiKeyRequest request,
        [FromServices] IValidator<AddApiKeyRequest> validator,
        DatabaseContext db,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid == false)
        {
            return new RazorComponentResult<AddApiKeyForm>(new { HasErrors = true });
        }

        var apiKey = new ApiKeyMapper().MapFromRequest(request);
        apiKey.IsActive = request.IsActive == "on";

        await db.Set<ApiKey>()
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsActive, !apiKey.IsActive), cancellationToken);
        await db.Set<ApiKey>().AddAsync(apiKey, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        context.Response.Headers.Append("HX-Redirect", "/");
        return Results.Empty;
    }
}