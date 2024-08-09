using Microsoft.AspNetCore.Http;

namespace Tunnelize.Shared.Routes;

public static class HttpContextExtensions
{
    public static IResult HtmxRedirect(
        this HttpContext context,
        string location)
    {
        context.Response.Headers.Append("HX-Redirect", location);
        return Results.Empty;
    }
}