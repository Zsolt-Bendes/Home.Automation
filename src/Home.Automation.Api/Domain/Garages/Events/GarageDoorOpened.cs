namespace Home.Automation.Api.Domain.Garages.Events;

public sealed record GarageDoorOpened(Guid GarageId, DateTimeOffset HappenedAt);
