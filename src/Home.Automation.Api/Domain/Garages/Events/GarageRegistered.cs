namespace Home.Automation.Api.Domain.Garages.Events;

public sealed record GarageRegistered(Guid GarageId, GarageDoorStatus DoorStatus, DateTimeOffset HappenedAt);
