using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Tunnelize.Server.Components.Authentication;

namespace Tunnelize.Server.Routes.Authentication.Login;

public class Login : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/authentication/login", Handle);
    }

    private static async Task<IResult> Handle(
        [FromForm] LoginRequest request,
        IAuthenticationService authenticationService,
        IValidator<LoginRequest> validator,
        HttpContext context)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (validationResult.IsValid == false)
        {
            var isEmailInvalid = validationResult.Errors.Any(x => x.ErrorCode == LoginErrorCodes.EmailInvalid);
            var isPasswordInvalid = validationResult.Errors.Any(x => x.ErrorCode == LoginErrorCodes.PasswordInvalid);
            return new RazorComponentResult<LoginForm>(new { isEmailInvalid, isPasswordInvalid });
        }

        context.Response.Headers.Append("HX-Redirect", "/dashboard");
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, "denis.pav@hotmail.com"), new Claim(ClaimTypes.Name, "denis.pav@hotmail.com")
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(claimsIdentity);
        await authenticationService.SignInAsync(context, CookieAuthenticationDefaults.AuthenticationScheme, principal,
            null);

        return TypedResults.Empty;
    }
}