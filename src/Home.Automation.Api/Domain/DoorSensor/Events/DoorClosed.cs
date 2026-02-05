namespace Home.Automation.Api.Domain.Devices.Events;

public sealed record DoorClosed(Guid SensorId, DateTimeOffset HappenedAt);
