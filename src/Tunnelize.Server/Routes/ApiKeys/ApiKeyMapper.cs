using Riok.Mapperly.Abstractions;
using Tunnelize.Server.Persistence.Entities;
using Tunnelize.Server.Routes.ApiKeys.CreateApiKeys;

namespace Tunnelize.Server.Routes.ApiKeys;

[Mapper]
public partial class ApiKeyMapper
{
    public partial ApiKey MapFromRequest(CreateApiKeyRequest request);
    public partial ApiKeyResponse MapToResponse(ApiKey apiKey);
}