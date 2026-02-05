using Home.Automation.Api.Domain.Devices.ValueObjects;
using Home.Automation.Api.Domain.DoorSensor.ValueObjects;

namespace Home.Automation.Api.Domain.Devices.Events;

public sealed record DoorSensorRegistered(
    Guid Id,
    Label Label,
    DoorStatus DoorStatus,
    bool SendNotification,
    TimeSpan? OpenReminderTimeSpan);
