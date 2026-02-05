using Home.Automation.Api.Domain.DoorSensor.ValueObjects;

namespace Home.Automation.Api.Domain.TempAndHumiditySensors.Events;

public sealed record TemperatureMeasurementReceived(
    Guid SensorId,
    double TemperatureInCelsius,
    double Humidity,
    DateTimeOffset MeasuredAt);

public sealed record TemperatureSensorRegistered(Guid SensorId, Label Name);
