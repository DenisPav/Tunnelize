using FluentValidation;
using Injectio.Attributes;

namespace Tunnelize.Server.Routes.Authentication.LoginCode;

[RegisterSingleton]
public class LoginCodeRequestValidator : AbstractValidator<LoginCodeRequest>
{
    public LoginCodeRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithErrorCode(LoginCodeErrorCodes.CodeInvalid);
    }
}

public record LoginCodeRequest(string Code);

public static class LoginCodeErrorCodes
{
    public const string CodeInvalid = nameof(CodeInvalid);
}