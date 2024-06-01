using System.Security.Cryptography;
using Injectio.Attributes;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;

namespace Tunnelize.Server.Authentication;

[RegisterScoped]
public class AuthCodeGenerator(DatabaseContext db) : IAuthCodeGenerator
{
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

        await db.Set<UserCode>().AddAsync(userCode, cancellationToken);
        return userCode;
    }
    
    private static string GenerateAuthCode()
    {
        var codeBytes = RandomNumberGenerator.GetBytes(6);
        var characters = codeBytes.Select(x =>
            {
                var result = x % AlphabetLength;
                return CodeAlphabet[result];
            })
            .Aggregate(string.Empty, (agg, character) => agg + character);

        return characters;
    }
}