﻿namespace Tunnelize.Client.Routes.ApiKeys.AddApiKey;

public class AddApiKeyRequest
{
    public string Value { get; init; }
    public string Description { get; init; }
    public string IsActive { get; init; }
}