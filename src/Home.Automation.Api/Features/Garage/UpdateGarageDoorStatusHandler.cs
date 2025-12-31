using Home.Automation.Api.Domain.Garages;
using Home.Automation.Api.Domain.Garages.Events;
using Home.Automation.Api.Services.Email;
using Wolverine.Marten;

namespace Home.Automation.Api.Features.Garage;

public sealed record UpdateGarageDoorStatus(Guid GarageId, GarageDoorStatus DoorStatus);

public static class UpdateGarageDoorStatusHandler
{
    public static Events Handle(
        UpdateGarageDoorStatus command,
        [WriteAggregate(nameof(UpdateGarageDoorStatus.GarageId))] Domain.Garages.Garage garage,
        TimeProvider timeProvider)
    {
        if (command.DoorStatus is GarageDoorStatus.Open)
        {
            return [new GarageDoorOpened(command.GarageId, timeProvider.GetLocalNow())];
        }

        return [new GarageDoorClosed(command.GarageId, timeProvider.GetLocalNow())];
    }
}

public static class GarageDoorOpenedHandler
{
    public static async Task Handle(
        GarageDoorOpened evt,
        IEmailService emailService,
        CancellationToken cancellationToken)
    {
        await emailService.SendGarageDoorStateChangeMailAsync(GarageDoorStatus.Open, evt.HappenedAt, cancellationToken);
    }
}

public static class GarageDoorClosedHandler
{
    public static async Task Handle(
        GarageDoorClosed evt,
        IEmailService emailService,
        CancellationToken cancellationToken)
    {
        await emailService.SendGarageDoorStateChangeMailAsync(GarageDoorStatus.Closed, evt.HappenedAt, cancellationToken);
    }
}