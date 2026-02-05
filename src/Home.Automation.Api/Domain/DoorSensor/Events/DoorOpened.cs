namespace Home.Automation.Api.Domain.Devices.Events;

public sealed record DoorOpened(Guid SensorId, DateTimeOffset HappenedAt);
