using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
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

    async Task Handle(
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
            // var isCodeInvalid = validationResult.Errors.Any(x => x.ErrorCode == LoginCodeErrorCodes.CodeInvalid);
            // return new RazorComponentResult<LoginCodeForm>(new { isEmailInvalid, isPasswordInvalid });
        }

        var cookieResult = await authenticationService.AuthenticateAsync(context, "intermediateCookie");
        var currentUserId = cookieResult.Principal?.GetUserId();
        
        var currentDateTime = DateTime.UtcNow;
        var userCodeQuery = db.Set<UserCode>()
            .Where(x => x.UserId == currentUserId && x.Code == request.Code && x.Expiration <= currentDateTime);
        var isValidLoginCodeRequest = await userCodeQuery.AnyAsync(cancellationToken);
        var targetedUser = await db.Set<User>()
            .FindAsync([currentUserId], cancellationToken);

        if (isValidLoginCodeRequest == false || targetedUser is null)
        {
            //return errror
        }

        await authenticationService.SignOutAsync(context, "intermediateCookie", null);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, targetedUser.Id.ToString()), 
            new Claim(ClaimTypes.Name, targetedUser.Email)
        };
        var claimsIdentity = new ClaimsIdentity(claims, "loginCookie");
        var principal = new ClaimsPrincipal(claimsIdentity);
        await authenticationService.SignInAsync(context, "loginCookie", principal,
            null);

        await userCodeQuery.ExecuteDeleteAsync(cancellationToken);
        
        context.Response.Headers.Append("HX-Redirect", "/dashboard");
    }
}