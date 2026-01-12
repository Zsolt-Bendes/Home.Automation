namespace Home.Automation.Api.Domain.Rooms.Events;

public sealed record TemperatureMeasurementReceived(
    Guid RoomId,
    double TemperatureInCelsius,
    double Humidity);
