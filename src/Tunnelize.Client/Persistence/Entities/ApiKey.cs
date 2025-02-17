﻿namespace Tunnelize.Client.Persistence.Entities;

public class ApiKey
{
    public Guid Id { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
}