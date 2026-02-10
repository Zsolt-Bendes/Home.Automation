using Home.Automation.Api.Domain.Devices.ValueObjects;
using Microsoft.AspNetCore.SignalR;

namespace Home.Automation.Api.Services;

public class LiveUpdater : Hub
{
    public async Task SendTemperatureUpdateAsync(
        Guid sensorId,
        double temperature,
        double humidity,
        double maxTemp,
        double minTemp,
        double avgTemp,
        double maxHumidity,
        double minHumidity,
        double avgHumidity,
        CancellationToken cancellationToken)
    {
        await Clients.All.SendAsync(
            "room_temp_update",
            new LiveTempData(
                sensorId,
                temperature,
                humidity,
                maxTemp,
                minTemp,
                avgTemp,
                maxHumidity,
                minHumidity,
                avgHumidity),
            cancellationToken);
    }

    //public async Task SendDoorStatusUpdateAsync(
    //    Guid sensorId,
    //    DoorStatus doorStatus,
    //    DateTimeOffset? openedAt,
    //    CancellationToken cancellationToken)
    //{
    //    await Clients.All.SendAsync(
    //        "door_sensor_update",
    //        new LiveDoorStatusData(
    //            sensorId,
    //            doorStatus,
    //            openedAt),
    //        cancellationToken);
    //}
}

public sealed record LiveTempData(
    Guid SensorId,
    double Temperature,
    double Humidity,
    double MaxTemp,
    double MinTemp,
    double AvgTemp,
    double MaxHumidity,
    double MinHumidity,
    double AvgHumidity);

public sealed record LiveDoorStatusData(
    Guid SensorId,
    DoorStatus DoorStatus,
    DateTimeOffset HappenedAt);