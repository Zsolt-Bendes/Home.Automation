using Home.Automation.Api.Domain.Common;
using Throw;

namespace Home.Automation.Api.Domain.Devices.ValueObjects;

public sealed class TemperatureAndHumidityMeasurement(
    double temperature,
    double humidity,
    DateTimeOffset measuredAt) : ValueObject
{
    public double Temperature { get; } = temperature;

    public double Humidity { get; } = humidity.Throw()
            .IfGreaterThanOrEqualTo(100)
            .IfLessThanOrEqualTo(0);

    public DateTimeOffset MeasuredAt { get; } = measuredAt;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Temperature;
        yield return Humidity;
    }
}