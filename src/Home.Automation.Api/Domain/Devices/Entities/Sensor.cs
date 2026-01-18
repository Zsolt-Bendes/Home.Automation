using Home.Automation.Api.Domain.Devices.ValueObjects;

namespace Home.Automation.Api.Domain.Devices.Entities;

public class Sensor
{
    public Sensor(Guid id, SensorType type)
    {
        Id = id;
        Type = type;
    }

    public Guid Id { get; }

    public SensorType Type { get; }
}
