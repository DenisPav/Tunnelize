using System.Security.Cryptography;
using Injectio.Attributes;
using Tunnelize.Server.Emails;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;

namespace Tunnelize.Server.Codes;

[RegisterScoped]
public class CodeGenerator(
    IEmailSender emailSender,
    DatabaseContext db,
    ILogger<CodeGenerator> log) : ICodeGenerator
{
    private const short AuthCodeLength = 6;
    private const string CodeAlphabet = "ABCDEFGHIJKLMNOPRSTUVZXWQ123456789";
    private static readonly int AlphabetLength = CodeAlphabet.Length;

    public async Task<UserCode> GenerateAuthCode(
        User user,
        CancellationToken cancellationToken)
    {
        var code = GenerateCode(AuthCodeLength);
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

    public string GenerateWildCardDomainCode()
    {
        var subdomainCode = GenerateCode(10);
        return subdomainCode.ToLower();
    }

    private static string GenerateCode(int length)
    {
        var codeBytes = RandomNumberGenerator.GetBytes(length);
        var characters = codeBytes.Select(x =>
            {
                var result = x % AlphabetLength;
                return CodeAlphabet[result];
            })
            .Aggregate(string.Empty, (agg, character) => agg + character);

        return characters;
    }
}