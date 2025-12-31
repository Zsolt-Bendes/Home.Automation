using Home.Automation.Api.Domain.Garages;

namespace Home.Automation.Api.Features.Garage;

public sealed class GarageView
{
    public GarageView()
    {
    }

    public GarageView(Guid id, GarageDoorStatus doorStatus, DateTimeOffset happenedAt)
    {
        Id = id;
        DoorStatus = doorStatus;
        if (doorStatus is GarageDoorStatus.Open)
        {
            OpenedAt = happenedAt;
        }
        else
        {
            ClosedAt = happenedAt;
        }
    }

    public Guid Id { get; set; }

    public GarageDoorStatus DoorStatus { get; set; }

    public DateTimeOffset? OpenedAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }
}
