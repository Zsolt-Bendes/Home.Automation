using Home.Automation.Api.Domain.Devices.ValueObjects;

namespace Home.Automation.Api.Domain.Devices.Entities;

public sealed class TemperatureAndHumiditySensor : Sensor
{
    public TemperatureAndHumiditySensor(Guid id, SensorType type, TemperatureAndHumidityMeasurement measurement)
        : base(id, type)
    {
        Measurement = measurement;
    }

    public TemperatureAndHumidityMeasurement Measurement { get; set; }
}