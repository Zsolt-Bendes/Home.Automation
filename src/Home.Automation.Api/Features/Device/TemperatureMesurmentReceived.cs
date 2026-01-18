using Home.Automation.Api.Domain.Devices.Events;
using Home.Automation.Api.Services;
using Microsoft.AspNetCore.SignalR;
using Wolverine.Marten;

namespace Home.Automation.Api.Features.Device;

public sealed record TemperatureMeasurement(Guid DeviceId, double Temperature, double Humidity);

public static class TemperatureMeasurementHandler
{
    public static async Task<TemperatureMeasurementReceived> Handle(
        TemperatureMeasurement command,
        [WriteAggregate(nameof(TemperatureMeasurement.DeviceId))] Domain.Devices.Device room,
        IHubContext<LiveUpdater> liveUpdater,
        CancellationToken cancellationToken)
    {
        await liveUpdater.Clients.All.SendAsync(
            "room_temp_update",
            new LiveTempData(
                command.DeviceId,
                command.Temperature,
                command.Humidity),
            cancellationToken);
        return new TemperatureMeasurementReceived(command.DeviceId, command.Temperature, command.Humidity);
    }
}
