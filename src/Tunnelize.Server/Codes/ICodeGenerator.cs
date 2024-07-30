using Tunnelize.Server.Persistence.Entities;

namespace Tunnelize.Server.Codes;

public interface ICodeGenerator
{
    Task<UserCode> GenerateAuthCode(
        User user,
        CancellationToken cancellationToken);

    string GenerateWildCardDomainCode();
}