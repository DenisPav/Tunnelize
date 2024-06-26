using System.Security.Cryptography;
using Injectio.Attributes;
using Tunnelize.Server.Emails;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;

namespace Tunnelize.Server.Authentication;

[RegisterScoped]
public class AuthCodeGenerator(
    IEmailSender emailSender,
    DatabaseContext db,
    ILogger<AuthCodeGenerator> log) : IAuthCodeGenerator
{
    private const short AuthCodeLength = 6;
    private const string CodeAlphabet = "ABCDEFGHIJKLMNOPRSTUVZXWQ123456789";
    private static readonly int AlphabetLength = CodeAlphabet.Length;

    public async Task<UserCode> Generate(
        User user,
        CancellationToken cancellationToken)
    {
        var code = GenerateAuthCode();
        var userCode = new UserCode
        {
            Code = code,
            Expiration = DateTime.UtcNow.AddMinutes(30),
            User = user,
        };

        log.LogDebug("New code generated: [{code}]", code);
        await db.Set<UserCode>().AddAsync(userCode, cancellationToken);
        await emailSender.SendEmail(code, user.Email);
        
        return userCode;
    }

    private static string GenerateAuthCode()
    {
        var codeBytes = RandomNumberGenerator.GetBytes(AuthCodeLength);
        var characters = codeBytes.Select(x =>
            {
                var result = x % AlphabetLength;
                return CodeAlphabet[result];
            })
            .Aggregate(string.Empty, (agg, character) => agg + character);

        return characters;
    }
}