using FluentValidation;
using Injectio.Attributes;

namespace Tunnelize.Server.Routes.ApiKeys.CreateApiKeys;

[RegisterSingleton]
public class CreateApiKeyRequestValidator : AbstractValidator<CreateApiKeyRequest>
{
    public CreateApiKeyRequestValidator()
    {
        RuleFor(x => x.SubDomain)
            .NotEmpty();
    }
}