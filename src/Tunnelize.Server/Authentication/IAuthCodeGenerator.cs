using Tunnelize.Server.Persistence.Entities;

namespace Tunnelize.Server.Authentication;

public interface IAuthCodeGenerator
{
    Task<UserCode> Generate(
        User user,
        CancellationToken cancellationToken);
}