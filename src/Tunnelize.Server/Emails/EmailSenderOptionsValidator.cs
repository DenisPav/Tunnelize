using Microsoft.Extensions.Options;

namespace Tunnelize.Server.Emails;

public class EmailSenderOptionsValidator : IValidateOptions<EmailSenderOptions>
{
    public ValidateOptionsResult Validate(
        string? name, 
        EmailSenderOptions options)
    {
        var isValid = string.IsNullOrEmpty(options.ApiKey) == false
                      && string.IsNullOrEmpty(options.ApiKeySecret) == false;

        return isValid
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("Email options are not valid");
    }
}