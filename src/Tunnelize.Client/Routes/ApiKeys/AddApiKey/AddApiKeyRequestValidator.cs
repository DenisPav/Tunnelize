using FluentValidation;
using Injectio.Attributes;

namespace Tunnelize.Client.Routes.ApiKeys.AddApiKey;

[RegisterSingleton]
public class AddApiKeyRequestValidator : AbstractValidator<AddApiKeyRequest>
{
    public AddApiKeyRequestValidator()
    {
        RuleFor(x => x.ApiKey)
            .NotEmpty();
    }
}