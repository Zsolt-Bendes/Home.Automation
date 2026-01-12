using Home.Automation.Api.Domain.Rooms.Events;
using Home.Automation.Api.Domain.Rooms.ValueObjects;

namespace Home.Automation.Api.Domain.Rooms;

public sealed record Room(Guid Id, RoomName Name)
{
    public static Room Create(RoomRegistered roomRegistered) =>
        new(roomRegistered.Id, roomRegistered.Name);
}
