namespace Tunnelize.Client.Routes.ApiKeys;

public record ApiKeyResponse(Guid Id, Guid Value, string Description, bool IsActive);