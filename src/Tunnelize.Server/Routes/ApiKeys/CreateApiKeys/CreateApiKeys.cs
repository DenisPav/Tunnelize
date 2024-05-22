using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;

namespace Tunnelize.Server.Routes.ApiKeys.CreateApiKeys;

public class CreateApiKeys : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api-keys/create", Handle).DisableAntiforgery();
    }

    private async Task<IResult> Handle(
        [FromForm] CreateApiKeyRequest request,
        IValidator<CreateApiKeyRequest> validator,
        DatabaseContext db,
        CancellationToken cancellationToken,
        HttpContext context)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid == false) return Results.BadRequest();

        var entity = new ApiKeyMapper().MapFromRequest(request);

        //TODO: add validation that this doesn't exist and add unique index to DB
        //TODO: add simple registration for each of endpoints so we can extract this
        await db.Set<ApiKey>().AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        context.Response.Headers.Append("HX-Redirect", "/dashboard");
        return Results.Empty;
    }
}