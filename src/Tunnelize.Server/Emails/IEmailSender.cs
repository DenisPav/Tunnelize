namespace Tunnelize.Server.Emails;

public interface IEmailSender
{
    Task SendEmail(
        string content,
        string to);
}