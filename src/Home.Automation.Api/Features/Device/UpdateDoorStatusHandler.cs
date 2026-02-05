using Home.Automation.Api.Domain.Devices.Events;
using Home.Automation.Api.Domain.Devices.IntegrationMessages;
using Home.Automation.Api.Domain.Devices.ValueObjects;
using Home.Automation.Api.Domain.DoorSensor;
using Home.Automation.Api.Domain.DoorSensor.ValueObjects;
using Home.Automation.Api.Services.Email;
using Marten;
using Wolverine;
using Wolverine.Marten;

namespace Home.Automation.Api.Features.Device;

public sealed record UpdateGarageDoorStatus(
    Guid SensorId,
    DoorStatus DoorStatus,
    string? Label,
    bool? SendStatusChangeEmails,
    TimeSpan? OpenReminderTimeSpan);

public static class UpdateDoorStatusHandler
{
    public static async Task Handle(
        UpdateGarageDoorStatus command,
        IMessageBus messageBus,
        IDocumentSession documentSession,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var sensorStream = await documentSession
            .Events
            .FetchForWriting<DoorStatusSensor>(command.SensorId, cancellationToken);
        if (sensorStream.Aggregate is null)
        {
            documentSession.Events.StartStream<DoorStatusSensor>(
                command.SensorId,
                new DoorSensorRegistered(
                    command.SensorId,
                    new Label(command.Label!),
                    command.DoorStatus,
                    command.SendStatusChangeEmails!.Value,
                    command.OpenReminderTimeSpan!.Value));
        }

        if (command.DoorStatus is DoorStatus.Open)
        {
            var doorOpenedEvent = new DoorOpened(command.SensorId, timeProvider.GetLocalNow());
            sensorStream.AppendOne(doorOpenedEvent);

            await messageBus.PublishAsync(doorOpenedEvent);
        }
        else
        {
            var doorClosedEvent = new DoorClosed(command.SensorId, timeProvider.GetLocalNow());
            sensorStream.AppendOne(doorClosedEvent);

            await messageBus.PublishAsync(doorClosedEvent);
        }
    }
}

public static class DoorOpenedHandler
{
    public static async Task Handle(
        DoorOpened evt,
        [ReadAggregate(nameof(DoorClosed.SensorId))] DoorStatusSensor sensor,
        IEmailService emailService,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        if (sensor.SendNotification is false)
        {
            return;
        }

        await emailService.SendGarageDoorStateChangeMailAsync(
            DoorStatus.Open,
            evt.HappenedAt,
            cancellationToken);
        if (sensor.OpenReminderTimeSpan is not null)
        {
            await messageBus.SendAsync(
                new DoorNotClosed(evt.SensorId)
                .DelayedFor(sensor.OpenReminderTimeSpan.Value));
        }
    }
}

public static class DoorClosedHandler
{
    public static async Task Handle(
        DoorClosed evt,
        [ReadAggregate(nameof(DoorClosed.SensorId))] DoorStatusSensor sensor,
        IEmailService emailService,
        CancellationToken cancellationToken)
    {
        if (sensor.SendNotification is false)
        {
            return;
        }

        await emailService.SendGarageDoorStateChangeMailAsync(
            DoorStatus.Closed,
            evt.HappenedAt,
            cancellationToken);
    }
}

public static class DoorNotClosedHandler
{
    public static async Task Handle(
        DoorNotClosed evt,
        [ReadAggregate(nameof(DoorClosed.SensorId))] DoorStatusSensor sensor,
        IEmailService emailService,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        if (sensor.DoorStatus is DoorStatus.Open)
        {
            await emailService.SendGarageDoorOpenReminderMailAsync(cancellationToken);
            if (sensor.SendNotification && sensor?.OpenReminderTimeSpan is not null)
            {
                await messageBus.SendAsync(
                    new DoorNotClosed(evt.SensorId)
                    .DelayedFor(sensor.OpenReminderTimeSpan.Value));
            }
        }
    }
}