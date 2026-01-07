using Home.Automation.Api.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace Home.Automation.Api.Services.Email.Client;

public sealed class EmailHttpClient : IEmailHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailHttpClient> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailHttpClient(
        HttpClient httpClient,
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }

    public async Task SendMailAsync(
        IList<EmailRecipient> recipients,
        string subject,
        string text,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(text);

        if (recipients?.Any() != true)
        {
            throw new ArgumentException("Incorrect recipients configurations!");
        }

        var multipart = new MultipartFormDataContent
        {
            { new StringContent(_emailSettings.Sender), "from" },
            { new StringContent(BuildRecipientListString(recipients)), "to" },
            { new StringContent(subject), "subject" },
            { new StringContent(text), "text" }
        };

        var responseMessage = await _httpClient.PostAsync(_emailSettings.Path, multipart, cancellationToken);

        _logger.EmailSendToMailGun(responseMessage.StatusCode);
    }

    private static string BuildRecipientListString(IList<EmailRecipient> recipients)
    {
        var sb = new StringBuilder(recipients.Count);
        foreach (var recipient in recipients)
        {
            sb.Append(recipient.ToString() + ';');
        }

        return sb.ToString();
    }
}

public static partial class Log
{
    [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Information,
    Message = "Sending garage door email. E-mail service response status: `{emailStatusCode}`")]
    public static partial void EmailSendToMailGun(this ILogger logger, HttpStatusCode emailStatusCode);
}