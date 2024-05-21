using FluentValidation;
using Injectio.Attributes;

namespace Tunnelize.Server.Routes.Authentication;

public record LoginRequest(string Email, string Password);

[RegisterSingleton]
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Email)
            .NotEmpty();
    }
}