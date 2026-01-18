namespace Home.Automation.Api.Domain.Devices.Events;

public sealed record TemperatureMeasurementReceived(
    Guid DeviceId,
    double TemperatureInCelsius,
    double Humidity);
