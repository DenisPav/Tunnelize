namespace Tunnelize.Server.Persistence.Entities;

public class UserCode
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public DateTime Expiration { get; set; }
    public Guid UserId { get; set; }
    
    public User User { get; set; }
}