using Home.Automation.Api.Domain.Devices.ValueObjects;

namespace Home.Automation.Api.Domain.Devices.Entities;

public sealed class DoorStatusSensor : Sensor
{
    public DoorStatusSensor(
        Guid id,
        DoorStatus doorStatus,
        SensorType type,
        bool sendNotification,
        TimeSpan? openReminderTimeSpan)
        : base(id, type)
    {
        DoorStatus = doorStatus;
        SendNotification = sendNotification;
        OpenReminderTimeSpan = openReminderTimeSpan;
    }

    public DoorStatus DoorStatus { get; private set; }

    public bool SendNotification { get; private set; }

    public TimeSpan? OpenReminderTimeSpan { get; private set; }

    public static DoorStatusSensor Create(
        Guid id,
        DoorStatus doorStatus,
        bool sendNotification,
        TimeSpan? openReminderTimeSpan) =>
        new DoorStatusSensor(
            id,
            doorStatus,
            SensorType.Door,
            sendNotification,
            openReminderTimeSpan);

    public void UpdateDoorStatusTo(DoorStatus status)
    {
        DoorStatus = status;
    }
}
