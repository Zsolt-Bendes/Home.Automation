using Home.Automation.Api.Domain.Devices.Entities;
using Home.Automation.Api.Domain.Devices.Events;
using Home.Automation.Api.Domain.Devices.IntegrationMessages;
using Home.Automation.Api.Domain.Devices.ValueObjects;
using Home.Automation.Api.Services.Email;
using Wolverine;
using Wolverine.Marten;

namespace Home.Automation.Api.Features.Device;

public sealed record UpdateGarageDoorStatus(Guid DeviceId, DoorStatus DoorStatus);

public static class UpdateDoorStatusHandler
{
    public static Events Handle(
        UpdateGarageDoorStatus command,
        [WriteAggregate(nameof(UpdateGarageDoorStatus.DeviceId))] Domain.Devices.Device device,
        TimeProvider timeProvider)
    {
        if (command.DoorStatus is DoorStatus.Open)
        {
            return [new DoorOpened(command.DeviceId, timeProvider.GetLocalNow())];
        }

        return [new DoorClosed(command.DeviceId, timeProvider.GetLocalNow())];
    }
}

public static class DoorOpenedHandler
{
    public static async Task Handle(
        DoorOpened evt,
        [ReadAggregate] Domain.Devices.Device device,
        IEmailService emailService,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        var sensor = device.Sensors.FirstOrDefault(_ => _.Type == SensorType.Door) as DoorStatusSensor;
        if (sensor?.SendNotification is false)
        {
            return;
        }

        await emailService.SendGarageDoorStateChangeMailAsync(DoorStatus.Open, evt.HappenedAt, cancellationToken);
        if (sensor?.OpenReminderTimeSpan is not null)
        {
            await messageBus.SendAsync(
                new DoorNotClosed(evt.DeviceId)
                .DelayedFor(sensor.OpenReminderTimeSpan.Value));
        }
    }
}

public static class DoorClosedHandler
{
    public static async Task Handle(
        DoorClosed evt,
        [ReadAggregate] Domain.Devices.Device device,
        IEmailService emailService,
        CancellationToken cancellationToken)
    {
        var sensor = device.Sensors.FirstOrDefault(_ => _.Type == SensorType.Door) as DoorStatusSensor;
        if (sensor?.SendNotification is false)
        {
            return;
        }

        await emailService.SendGarageDoorStateChangeMailAsync(DoorStatus.Closed, evt.HappenedAt, cancellationToken);
    }
}

public static class DoorNotClosedHandler
{
    public static async Task Handle(
        DoorNotClosed evt,
        [ReadAggregate] Domain.Devices.Device device,
        IEmailService emailService,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        var sensor = device.Sensors.FirstOrDefault(_ => _.Type == SensorType.Door) as DoorStatusSensor;
        if (sensor?.DoorStatus is DoorStatus.Open)
        {
            await emailService.SendGarageDoorOpenReminderMailAsync(cancellationToken);
            if (sensor?.SendNotification is true && sensor?.OpenReminderTimeSpan is not null)
            {
                await messageBus.SendAsync(
                    new DoorNotClosed(evt.DeviceId)
                    .DelayedFor(sensor.OpenReminderTimeSpan.Value));
            }
        }
    }
}