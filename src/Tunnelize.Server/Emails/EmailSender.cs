using Injectio.Attributes;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;

namespace Tunnelize.Server.Emails;

[RegisterScoped]
public class EmailSender(ILogger<EmailSender> log) : IEmailSender
{
    public async Task SendEmail(
        string content,
        string to)
    {
        var client = new MailjetClient(
            "-",
            "-");

        var email = new TransactionalEmailBuilder()
            .WithFrom(new SendContact("denis.pav@hotmail.com"))
            .WithSubject("Code")
            .WithTextPart(content)
            .WithTo(new SendContact(to))
            .Build();

        log.LogDebug("Sending email!");
        var response = await client.SendTransactionalEmailAsync(email);
        var hasErrors = response.Messages is null
                        || response.Messages.Any(x => x.Errors.Count != 0);
        if (hasErrors)
        {
            log.LogWarning("Failed sending email: [{reason}]",
                response.Messages?[0].Errors?[0].ErrorMessage ?? "invalid credentials");
        }
    }
}