using Home.Automation.Api.Domain.Rooms.Events;
using Home.Automation.Api.Services;
using Wolverine.Marten;

namespace Home.Automation.Api.Features.Room;

public sealed record TemperatureMeasurement(Guid RoomId, double Temperature, double Humidity);

public static class TemperatureMeasurementHandler
{
    public static async Task<TemperatureMeasurementReceived> Handle(
        TemperatureMeasurement command,
        [WriteAggregate(nameof(TemperatureMeasurement.RoomId))] Domain.Rooms.Room room,
        LiveUpdater liveUpdater,
        CancellationToken cancellationToken)
    {
        await liveUpdater.SendTemperatureUpdateAsync(command.RoomId, command.Temperature, command.Humidity, cancellationToken);
        return new TemperatureMeasurementReceived(command.RoomId, command.Temperature, command.Humidity);
    }
}
