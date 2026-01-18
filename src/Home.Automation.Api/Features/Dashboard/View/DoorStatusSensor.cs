using Home.Automation.Api.Domain.Devices.ValueObjects;

namespace Home.Automation.Api.Features.Dashboard.View;

public sealed class DoorStatusSensor : SensorsViewBase
{
    public DoorStatusSensor(
        Guid id,
        string name,
        DoorStatus doorStatus,
        DateTimeOffset? openedAt,
        DateTimeOffset? closedAt)
        : base(id, name)
    {
        DoorStatus = doorStatus;
        OpenedAt = openedAt;
        ClosedAt = closedAt;
    }

    public DoorStatus DoorStatus { get; set; }

    public DateTimeOffset? OpenedAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }
}
