using Injectio.Attributes;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Mailjet.Client.TransactionalEmails;

namespace Tunnelize.Server.Emails;

public interface IEmailSender
{
    Task SendEmail(
        string content,
        string to);
}

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
        log.LogDebug("Email sent!");
    }
}