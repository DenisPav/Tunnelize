using Riok.Mapperly.Abstractions;
using Tunnelize.Client.Persistence.Entities;
using Tunnelize.Client.Routes.ApiKeys.AddApiKey;

namespace Tunnelize.Client.Routes.ApiKeys;

[Mapper]
public partial class ApiKeyMapper
{
    [MapperIgnoreSource(nameof(AddApiKeyRequest.IsActive))]
    public partial ApiKey MapFromRequest(AddApiKeyRequest request);
    public partial ApiKeyResponse MapToResponse(ApiKey apiKey);
}