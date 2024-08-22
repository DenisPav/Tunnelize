namespace Tunnelize.Client.Routes.ApiKeys;

public record ApiKeyResponse(Guid Id, string Value, string Description, bool IsActive);