using FluentValidation;
using Injectio.Attributes;
using Microsoft.EntityFrameworkCore;
using Tunnelize.Client.Persistence;
using Tunnelize.Client.Persistence.Entities;

namespace Tunnelize.Client.Routes.ApiKeys.AddApiKey;

[RegisterScoped]
public class AddApiKeyRequestValidator : AbstractValidator<AddApiKeyRequest>
{
    private readonly DatabaseContext _db;

    public AddApiKeyRequestValidator(DatabaseContext db)
    {
        _db = db;
        
        RuleFor(x => x.Value)
            .NotEmpty()
            .MustAsync(IsNotAlreadyAdded);
    }

    private async Task<bool> IsNotAlreadyAdded(
        string key, 
        CancellationToken cancellationToken)
    {
        var apiKeyAlreadyUsed = await _db.Set<ApiKey>()
            .AnyAsync(x => x.Value == key, cancellationToken);

        return apiKeyAlreadyUsed == false;
    }
}