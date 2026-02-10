using Home.Automation.Api.Domain.TempAndHumiditySensors;
using Home.Automation.Api.Domain.TempAndHumiditySensors.Events;
using Home.Automation.Api.Services;
using Microsoft.AspNetCore.SignalR;
using Wolverine.Attributes;
using Wolverine.Marten;

namespace Home.Automation.Api.Features.Device;

public sealed record TemperatureMeasurement(Guid SensorId, double Temperature, double Humidity);

public static class TemperatureMeasurementHandler
{
    public static TemperatureMeasurementReceived Handle(
        TemperatureMeasurement command,
        [WriteAggregate(nameof(TemperatureMeasurement.SensorId))] TemperatureAndHumiditySensor sensor,
        TimeProvider timeProvider)
    {
        return new TemperatureMeasurementReceived(
            command.SensorId,
            command.Temperature,
            command.Humidity,
            timeProvider.GetLocalNow());
    }

    [WolverineAfter]
    public static async Task SendLiveUpdate(
        TemperatureMeasurement command,
        [ReadAggregate(nameof(TemperatureMeasurement.SensorId))] TemperatureAndHumiditySensor sensor,
        IHubContext<LiveUpdater> liveUpdater,
        CancellationToken cancellationToken)
    {
        await liveUpdater.Clients.All.SendAsync(
            "room_temp_update",
            new LiveTempData(
                command.SensorId,
                command.Temperature,
                command.Humidity,
                sensor.MaxTemperature,
                sensor.MinTemperature,
                sensor.AverageTemperature,
                sensor.MaxHumidity,
                sensor.MinHumidity,
                sensor.AverageHumidity),
            cancellationToken);
    }
}

