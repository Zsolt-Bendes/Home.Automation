using Home.Automation.Api.Domain.Devices.ValueObjects;

namespace Home.Automation.Api.Services.Email;

public interface IEmailService
{
    Task SendGarageDoorStateChangeMailAsync(
        DoorStatus doorStatus,
        DateTimeOffset happenedAt,
        CancellationToken cancellationToken = default);

    Task SendGarageDoorOpenReminderMailAsync(CancellationToken cancellationToken = default);
}
