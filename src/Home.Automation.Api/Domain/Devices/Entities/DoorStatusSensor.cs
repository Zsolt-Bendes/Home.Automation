using Home.Automation.Api.Domain.Devices.ValueObjects;

namespace Home.Automation.Api.Domain.Devices.Entities;

public sealed class DoorStatusSensor : Sensor
{
    public DoorStatusSensor(
        Guid id,
        DoorStatus doorStatus,
        SensorType type,
        bool sendNotification)
        : base(id, type)
    {
        DoorStatus = doorStatus;
        SendNotification = sendNotification;
    }

    public DoorStatus DoorStatus { get; set; }

    public bool SendNotification { get; set; }
}
