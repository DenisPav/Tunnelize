namespace Tunnelize.Server.Routes.ApiKeys;

public record ApiKeyResponse(Guid Id, string Key, string SubDomain);