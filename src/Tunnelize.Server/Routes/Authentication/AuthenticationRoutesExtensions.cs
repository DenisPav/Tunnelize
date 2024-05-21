using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Tunnelize.Server.Routes.Authentication;

public static class AuthenticationRoutesExtensions
{
    public static void MapAuthRoutes(this IEndpointRouteBuilder app)
    {
        var authRoutes = app.MapGroup("authentication");

        authRoutes.MapPost("/login", async (
            [FromForm] LoginRequest request,
            IAuthenticationService authenticationService,
            IValidator<LoginRequest> validator,
            HttpContext context) =>
        {
            var validationResult = validator.Validate(request);
            if (validationResult.IsValid == false)
                return Results.BadRequest();

            context.Response.Headers.Append("HX-Redirect", "/dashboard");
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, "denis.pav@hotmail.com"),
                new Claim(ClaimTypes.Name, "denis.pav@hotmail.com")
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(claimsIdentity);
            await authenticationService.SignInAsync(context, CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                null);

            return TypedResults.Empty;
        });

        authRoutes.MapPost("/logout", async (
            HttpContext context,
            [FromServices] IAuthenticationService authenticationService) =>
        {
            context.Response.Headers.Append("HX-Redirect", "/login");
            await authenticationService.SignOutAsync(context, CookieAuthenticationDefaults.AuthenticationScheme, null);

            return TypedResults.Empty;
        });
    }
}