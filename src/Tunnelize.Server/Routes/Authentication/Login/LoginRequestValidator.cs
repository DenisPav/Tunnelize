using FluentValidation;
using Injectio.Attributes;

namespace Tunnelize.Server.Routes.Authentication.Login;

[RegisterSingleton]
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithErrorCode(LoginErrorCodes.EmailInvalid);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithErrorCode(LoginErrorCodes.PasswordInvalid);
    }
}