using Microsoft.AspNetCore.SignalR;

namespace Home.Automation.Api.Services;

public class LiveUpdater : Hub
{
    public async Task SendTemperatureUpdateAsync(
        Guid roomId,
        double temperature,
        double humidity,
        CancellationToken cancellationToken)
    {
        await Clients.All.SendAsync("room_temp_update", new LiveTempData(roomId, temperature, humidity), cancellationToken);
    }
}

public sealed record LiveTempData(Guid RoomId, double Temperature, double Humidity);