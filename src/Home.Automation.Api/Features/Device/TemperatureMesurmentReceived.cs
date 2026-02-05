using Home.Automation.Api.Domain.TempAndHumiditySensors;
using Home.Automation.Api.Domain.TempAndHumiditySensors.Events;
using Home.Automation.Api.Services;
using Microsoft.AspNetCore.SignalR;
using Wolverine.Marten;

namespace Home.Automation.Api.Features.Device;

public sealed record TemperatureMeasurement(Guid SensorId, double Temperature, double Humidity);

public static class TemperatureMeasurementHandler
{
    public static async Task<TemperatureMeasurementReceived> Handle(
        TemperatureMeasurement command,
        [WriteAggregate(nameof(TemperatureMeasurement.SensorId))] TemperatureAndHumiditySensor sensor,
        IHubContext<LiveUpdater> liveUpdater,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        await liveUpdater.Clients.All.SendAsync(
            "room_temp_update",
            new LiveTempData(
                command.SensorId,
                command.Temperature,
                command.Humidity),
            cancellationToken);

        return new TemperatureMeasurementReceived(
            command.SensorId,
            command.Temperature,
            command.Humidity,
            timeProvider.GetLocalNow());
    }
}

