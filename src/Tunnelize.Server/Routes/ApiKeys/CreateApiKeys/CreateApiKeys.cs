using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Tunnelize.Server.Codes;
using Tunnelize.Server.Components.ApiKeys;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Server.Routes.ApiKeys.CreateApiKeys;

public class CreateApiKeys : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/api-keys/create", Handle).DisableAntiforgery();
    }

    private static async Task<IResult> Handle(
        [FromForm] CreateApiKeyRequest request,
        IValidator<CreateApiKeyRequest> validator,
        ICodeGenerator codeGenerator,
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
        entity.Key = codeGenerator.GenerateApiKeyCode();

        await db.Set<ApiKey>().AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return context.HtmxRedirect("/dashboard");
    }
}