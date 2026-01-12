using Home.Automation.Api.Domain.Rooms.ValueObjects;

namespace Home.Automation.Api.Domain.Rooms.Events;


public sealed record RoomRegistered(Guid Id, RoomName Name, DateTimeOffset CreatedAt);
