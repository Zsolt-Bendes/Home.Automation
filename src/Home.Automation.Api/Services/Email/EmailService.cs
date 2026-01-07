using Home.Automation.Api.Domain.Garages;
using Home.Automation.Api.Infrastructure.Settings;
using Home.Automation.Api.Services.Email.Client;

namespace Home.Automation.Api.Services.Email;

public sealed class EmailService : IEmailService
{
    private const string GarageDoorSubject = "Garage door";

    private readonly IEmailHttpClient _emailClient;
    private readonly ILogger<EmailService> _logger;
    private readonly List<EmailRecipient> _emailRecipients;

    public EmailService(
        IEmailHttpClient emailClient,
        IConfiguration configuration,
        ILogger<EmailService> logger)
    {
        _emailClient = emailClient;
        _logger = logger;

        var recipients = configuration.GetSection("Recipients").Get<List<Recipient>>();
        if (recipients is null || recipients.Count == 0)
        {
            throw new ArgumentException("Incorrect recipients configurations!");
        }

        _emailRecipients = recipients.ConvertAll(x => new EmailRecipient(x.DisplayName, x.Email));
    }

    public async Task SendGarageDoorOpenReminderMailAsync(CancellationToken cancellationToken = default)
    {
        _logger.SendingReminderEmail();
        await _emailClient.SendMailAsync(
                _emailRecipients,
                GarageDoorSubject,
                "OPEN\nYour garage door is still open. Don't forget to close the door!",
                cancellationToken);
    }

    public async Task SendGarageDoorStateChangeMailAsync(
        GarageDoorStatus doorStatus,
        DateTimeOffset happenedAt,
        CancellationToken cancellationToken = default)
    {
        _logger.SendingGarageDoorStatusChangeEmail(doorStatus);
        if (doorStatus is GarageDoorStatus.Open)
        {
            await _emailClient.SendMailAsync(
                _emailRecipients,
                GarageDoorSubject,
                $"OPEN\nYour garage door has been opened at {happenedAt:yyyy-MM-dd HH:mm}!",
                cancellationToken);
        }
        else
        {
            await _emailClient.SendMailAsync(
                _emailRecipients,
                GarageDoorSubject,
                $"CLOSED\nYour garage door has been closed at {happenedAt:yyyy-MM-dd HH:mm}!",
                cancellationToken);
        }
        _logger.EmailSend();
    }
}

public static partial class Log
{
    [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Information,
    Message = "Sending garage door e-mail. Door status: `{doorStatus}`")]
    public static partial void SendingGarageDoorStatusChangeEmail(this ILogger logger, GarageDoorStatus doorStatus);

    [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "E-mail send successfully")]
    public static partial void EmailSend(this ILogger logger);

    [LoggerMessage(
    EventId = 3,
    Level = LogLevel.Information,
    Message = "Sending garage door reminder e-mail.")]
    public static partial void SendingReminderEmail(this ILogger logger);
}