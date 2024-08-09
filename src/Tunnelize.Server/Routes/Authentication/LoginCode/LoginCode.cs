using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tunnelize.Server.Authentication;
using Tunnelize.Server.Components.Authentication;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Server.Routes.Authentication.LoginCode;

public class LoginCode : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/authentication/login/code", Handle);
    }

    private static async Task<IResult> Handle(
        [FromForm] LoginCodeRequest request,
        IValidator<LoginCodeRequest> validator,
        DatabaseContext db,
        IAuthenticationService authenticationService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid == false)
        {
            return new RazorComponentResult<LoginWithCode>(new { HasErrors = true });
        }

        var cookieResult = await authenticationService.AuthenticateAsync(context, Schemes.IntermediateCookie);
        var currentUserId = cookieResult.Principal?.GetUserId();

        var currentDateTime = DateTime.UtcNow;
        var userCodeQuery = db.Set<UserCode>()
            .Where(x => x.UserId == currentUserId && x.Code == request.Code && x.Expiration >= currentDateTime);
        var isValidLoginCodeRequest = await userCodeQuery.AnyAsync(cancellationToken);
        var targetedUser = await db.Set<User>()
            .FindAsync([currentUserId], cancellationToken);

        if (isValidLoginCodeRequest == false || targetedUser is null)
        {
            return new RazorComponentResult<LoginWithCode>(new { HasErrors = true });
        }

        await authenticationService.SignOutAsync(context, Schemes.IntermediateCookie, null);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, targetedUser.Id.ToString()),
            new Claim(ClaimTypes.Name, targetedUser.Email)
        };
        var claimsIdentity = new ClaimsIdentity(claims, Schemes.LoginCookie);
        var principal = new ClaimsPrincipal(claimsIdentity);
        await authenticationService.SignInAsync(context, Schemes.LoginCookie, principal,
            null);

        await userCodeQuery.ExecuteDeleteAsync(cancellationToken);

        return context.HtmxRedirect("/dashboard");
    }
}