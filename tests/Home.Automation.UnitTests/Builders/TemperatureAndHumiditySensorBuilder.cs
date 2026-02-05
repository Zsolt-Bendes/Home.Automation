using Home.Automation.Api.Domain.Devices.ValueObjects;
using Home.Automation.Api.Domain.TempAndHumiditySensors;

namespace Home.Automation.UnitTests.Builders;

internal sealed class TemperatureAndHumiditySensorBuilder
{
    private Guid _id = Guid.NewGuid();
    private TemperatureAndHumidityMeasurement? _measurement;

    public TemperatureAndHumiditySensor Build() => new TemperatureAndHumiditySensor(_id, _measurement);

    public TemperatureAndHumiditySensorBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public TemperatureAndHumiditySensorBuilder WithMeasurements(TemperatureAndHumidityMeasurement? measurement)
    {
        _measurement = measurement;
        return this;
    }
}
