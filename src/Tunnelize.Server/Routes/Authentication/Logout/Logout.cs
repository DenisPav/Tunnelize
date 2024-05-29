using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Tunnelize.Server.Routes.Authentication.Logout;

public class Logout : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/authentication/logout", Handle);
    }

    private static async Task<EmptyHttpResult> Handle(
        HttpContext context,
        [FromServices] IAuthenticationService authenticationService)
    {
        context.Response.Headers.Append("HX-Redirect", "/login");
        await authenticationService.SignOutAsync(context, CookieAuthenticationDefaults.AuthenticationScheme, null);

        return TypedResults.Empty;
    }
}