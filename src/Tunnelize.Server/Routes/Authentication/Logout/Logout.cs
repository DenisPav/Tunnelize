using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Tunnelize.Server.Authentication;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Server.Routes.Authentication.Logout;

public class Logout : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/authentication/logout", Handle);
    }

    private static async Task<EmptyHttpResult> Handle(
        HttpContext context,
        [FromServices] IAuthenticationService authenticationService)
    {
        context.Response.Headers.Append("HX-Redirect", "/login");
        await authenticationService.SignOutAsync(context, Schemes.LoginCookie, null);

        return TypedResults.Empty;
    }
}