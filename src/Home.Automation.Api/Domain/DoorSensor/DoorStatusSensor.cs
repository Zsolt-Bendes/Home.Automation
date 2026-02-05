using Home.Automation.Api.Domain.Devices.Events;
using Home.Automation.Api.Domain.Devices.ValueObjects;
using Home.Automation.Api.Domain.DoorSensor.ValueObjects;

namespace Home.Automation.Api.Domain.DoorSensor;

public sealed record DoorStatusSensor(
        Guid Id,
        Label Label,
        DoorStatus DoorStatus,
        bool SendNotification,
        TimeSpan? OpenReminderTimeSpan)
{
    public static DoorStatusSensor Create(DoorSensorRegistered deviceRegistered) =>
        new(
            deviceRegistered.Id,
            deviceRegistered.Label,
            deviceRegistered.DoorStatus,
            deviceRegistered.SendNotification,
            deviceRegistered.OpenReminderTimeSpan);

    public static DoorStatusSensor Apply(DoorClosed doorClosed, DoorStatusSensor sensor)
    {
        if (sensor.DoorStatus is not DoorStatus.Open)
        {
            return sensor;
        }

        return sensor with { DoorStatus = DoorStatus.Closed };
    }

    public static DoorStatusSensor Apply(DoorOpened doorClosed, DoorStatusSensor sensor)
    {
        if (sensor.DoorStatus is not DoorStatus.Closed)
        {
            return sensor;
        }

        return sensor with { DoorStatus = DoorStatus.Open };
    }
}
