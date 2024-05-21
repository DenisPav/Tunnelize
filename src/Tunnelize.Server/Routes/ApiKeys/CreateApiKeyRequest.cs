using FluentValidation;
using Injectio.Attributes;

namespace Tunnelize.Server.Routes.ApiKeys;

public record CreateApiKeyRequest(string Subdomain);

[RegisterSingleton]
public class CreateApiKeyRequestValidator : AbstractValidator<CreateApiKeyRequest>
{
    public CreateApiKeyRequestValidator()
    {
        RuleFor(x => x.Subdomain)
            .NotEmpty();
    }
}