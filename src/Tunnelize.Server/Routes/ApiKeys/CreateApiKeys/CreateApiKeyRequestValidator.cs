using FluentValidation;
using Injectio.Attributes;
using Microsoft.EntityFrameworkCore;
using Tunnelize.Server.Persistence;
using Tunnelize.Server.Persistence.Entities;

namespace Tunnelize.Server.Routes.ApiKeys.CreateApiKeys;

[RegisterScoped]
public class CreateApiKeyRequestValidator: AbstractValidator<CreateApiKeyRequest>
{
    private readonly DatabaseContext _db;

    public CreateApiKeyRequestValidator(DatabaseContext db)
    {
        _db = db;

        RuleFor(x => x.SubDomain)
            .NotEmpty()
            .MustAsync(IsSubdomainTaken);
    }

    private async Task<bool> IsSubdomainTaken(
        string subdomain, 
        CancellationToken cancellationToken)
    {
        var subdomainExists =  await _db.Set<ApiKey>()
            .AnyAsync(x => x.SubDomain == subdomain, cancellationToken);

        return !subdomainExists;
    }
}