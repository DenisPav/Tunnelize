using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tunnelize.Server.Authentication;
using Tunnelize.Server.Codes;
using Tunnelize.Server.Components.Authentication;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;
using Tunnelize.Shared.Routes;

namespace Tunnelize.Server.Routes.Authentication.Login;

public class Login : IRouteMapper
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/authentication/login", Handle);
    }

    private static async Task<IResult> Handle(
        [FromForm] LoginRequest request,
        IAuthenticationService authenticationService,
        IValidator<LoginRequest> validator,
        ICodeGenerator authCodeGenerator,
        DatabaseContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid == false)
        {
            var isEmailInvalid = validationResult.Errors.Any(x => x.ErrorCode == LoginErrorCodes.EmailInvalid);
            var isPasswordInvalid = validationResult.Errors.Any(x => x.ErrorCode == LoginErrorCodes.PasswordInvalid);
            return new RazorComponentResult<LoginForm>(new { isEmailInvalid, isPasswordInvalid });
        }
        
        var hasher = new PasswordHasher<User>();
        var targetedUser = await db.Set<User>()
            .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
        if (targetedUser is null)
        {
            return new RazorComponentResult<LoginForm>(new { IsCombinationInvalid = true });
        }
        
        var verificationResult = hasher.VerifyHashedPassword(targetedUser, targetedUser.PasswordHash, request.Password);
        var isPasswordOkay = verificationResult == PasswordVerificationResult.Success;
        if (isPasswordOkay == false)
        {
            return new RazorComponentResult<LoginForm>(new { IsCombinationInvalid = true });
        }

        await authCodeGenerator.GenerateAuthCode(targetedUser, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        
        context.Response.Headers.Append("HX-Redirect", "/login/code");
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, targetedUser.Id.ToString()), 
            new Claim(ClaimTypes.Name, targetedUser.Email)
        };
        var claimsIdentity = new ClaimsIdentity(claims, Schemes.IntermediateCookie);
        var principal = new ClaimsPrincipal(claimsIdentity);
        await authenticationService.SignInAsync(context, Schemes.IntermediateCookie, principal,
            null);

        return TypedResults.Empty;
    }
}