using Home.Automation.Api.Domain.Garages;

namespace Home.Automation.Api.Services.Email;

public interface IEmailService
{
    Task SendGarageDoorStateChangeMailAsync(
        GarageDoorStatus doorStatus,
        DateTimeOffset happenedAt,
        CancellationToken cancellationToken = default);
}
