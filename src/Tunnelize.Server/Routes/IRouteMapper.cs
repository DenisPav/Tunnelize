namespace Tunnelize.Server.Routes;

public interface IRouteMapper
{
    void Map(IEndpointRouteBuilder builder);
}