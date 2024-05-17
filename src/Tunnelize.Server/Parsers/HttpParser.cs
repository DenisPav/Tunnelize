using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace Tunnelize.Server.Parsers;

public static class HttpParsers
{
    public static readonly TextParser<TextSpan> HostParser = (
        from _ in Span.ExceptIgnoreCase("host:")
        from headerValue in Span.EqualToIgnoreCase("host:")
            .IgnoreThen(Span.WhiteSpace.IgnoreThen(Span.WithAll(c =>
                char.IsLetterOrDigit(c) || c == '.' || c == ':')))
        select headerValue
    );
}