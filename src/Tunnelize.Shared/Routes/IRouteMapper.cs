using Microsoft.AspNetCore.Routing;

namespace Tunnelize.Shared.Routes;

public interface IRouteMapper
{
    void Map(IEndpointRouteBuilder builder);
}