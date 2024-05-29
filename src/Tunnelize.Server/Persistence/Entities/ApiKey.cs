namespace Tunnelize.Server.Persistence.Entities;

public class ApiKey
{
    public Guid Id { get; set; }
    public string SubDomain { get; set; }
    public Guid UserId { get; set; }
    
    public User User { get; set; }
}