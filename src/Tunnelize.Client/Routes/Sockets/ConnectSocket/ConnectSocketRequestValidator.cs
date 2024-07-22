using FluentValidation;
using Injectio.Attributes;

namespace Tunnelize.Client.Routes.Sockets.ConnectSocket;

[RegisterSingleton]
public class ConnectSocketRequestValidator : AbstractValidator<ConnectSocketRequest>
{
    public ConnectSocketRequestValidator()
    {
        RuleFor(x => x.Port)
            .NotEmpty();
    }
}