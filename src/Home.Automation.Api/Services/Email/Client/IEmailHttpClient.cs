namespace Home.Automation.Api.Services.Email.Client;

public interface IEmailHttpClient
{
    Task SendMailAsync(
        IList<EmailRecipient> recipients,
        string subject,
        string text,
        CancellationToken cancellationToken);
}