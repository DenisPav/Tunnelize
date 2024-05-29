namespace Tunnelize.Server.Persistence.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    
    public IEnumerable<ApiKey> ApiKeys { get; set; }
}