using Home.Automation.Api.Domain.Devices.Entities;
using Home.Automation.Api.Domain.Devices.Events;
using Home.Automation.Api.Domain.Devices.ValueObjects;

namespace Home.Automation.Api.Domain.Devices;

public sealed record Device(
    Guid Id,
    DeviceName Name,
    List<Sensor> Sensors)
{
    public static Device Create(DeviceRegistered deviceRegistered) =>
        new(deviceRegistered.Id, deviceRegistered.Name, []);

    public static void Apply(DoorClosed doorClosed, Device device)
    {
        var sensor = device.Sensors.FirstOrDefault(_ => _.Type == SensorType.Door) as DoorStatusSensor;
        if (sensor?.DoorStatus is not DoorStatus.Open)
        {
            return;
        }

        sensor.DoorStatus = DoorStatus.Closed;
    }

    public static void Apply(DoorOpened doorClosed, Device device)
    {
        var sensor = device.Sensors.FirstOrDefault(_ => _.Type == SensorType.Door) as DoorStatusSensor;
        if (sensor?.DoorStatus is not DoorStatus.Closed)
        {
            return;
        }

        sensor.DoorStatus = DoorStatus.Open;
    }
}
