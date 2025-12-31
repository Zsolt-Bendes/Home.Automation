namespace Home.Automation.Api.Domain.Garages.Events;

public sealed record GarageDoorClosed(Guid GarageId, DateTimeOffset HappenedAt);
