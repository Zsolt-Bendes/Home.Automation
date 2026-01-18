using Home.Automation.Api.Domain.Devices.Entities;
using Home.Automation.Api.Domain.Devices.ValueObjects;

namespace Home.Automation.Api.Domain.Devices.Events;

public sealed record DeviceRegistered(
    Guid Id,
    DeviceName Name,
    SensorType DeviceType,
    List<Sensor> Sensors,
    DateTimeOffset CreatedAt);
