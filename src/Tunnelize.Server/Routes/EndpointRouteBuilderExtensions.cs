namespace Tunnelize.Server.Routes;

public static class EndpointRouteBuilderExtensions
{
    public static void MapRoute<TRouteMapper>(this IEndpointRouteBuilder builder)
        where TRouteMapper : IRouteMapper, new()
    {
        new TRouteMapper().Map(builder);
    }
}