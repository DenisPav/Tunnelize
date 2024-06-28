﻿using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tunnelize.Server.Authentication;
using Tunnelize.Server.Components.Authentication;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;

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
        IAuthCodeGenerator authCodeGenerator,
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

        await authCodeGenerator.Generate(targetedUser, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        
        context.Response.Headers.Append("HX-Redirect", "/login/code");
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, targetedUser.Id.ToString()), 
            new Claim(ClaimTypes.Name, targetedUser.Email)
        };
        var claimsIdentity = new ClaimsIdentity(claims, "intermediateCookie");
        var principal = new ClaimsPrincipal(claimsIdentity);
        await authenticationService.SignInAsync(context, "intermediateCookie", principal,
            null);

        return TypedResults.Empty;
    }
}