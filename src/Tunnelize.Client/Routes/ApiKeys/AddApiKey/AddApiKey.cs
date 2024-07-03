using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Tunnelize.Client.Components.ApiKeys;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Client.Routes.ApiKeys.AddApiKey;

public class AddApiKey : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/api-keys", Handle);
    }

    private static async Task<IResult> Handle(
        [FromForm] AddApiKeyRequest request,
        [FromServices] IValidator<AddApiKeyRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid == false)
        {
            return new RazorComponentResult<AddApiKeyForm>(new { HasErrors = true });
        }

        //todo save and return back to dashboard for api keys
        return Results.Empty;
    }
}