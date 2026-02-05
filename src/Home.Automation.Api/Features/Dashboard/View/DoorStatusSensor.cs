using Home.Automation.Api.Domain.Devices.ValueObjects;

namespace Home.Automation.Api.Features.Dashboard.View;

public sealed class DoorStatusSensor : SensorsViewBase
{
    public DoorStatusSensor(
        Guid id,
        string name,
        DoorStatus doorStatus,
        bool sendReminderEmail,
        TimeSpan? reminderTimeSpan,
        DateTimeOffset? openedAt,
        DateTimeOffset? closedAt)
        : base(id, name)
    {
        DoorStatus = doorStatus;
        SendReminderEmail = sendReminderEmail;
        ReminderTimeSpan = reminderTimeSpan;
        OpenedAt = openedAt;
        ClosedAt = closedAt;
    }

    public DoorStatus DoorStatus { get; set; }

    public bool SendReminderEmail { get; set; }
    public TimeSpan? ReminderTimeSpan { get; }
    public DateTimeOffset? OpenedAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }
}
