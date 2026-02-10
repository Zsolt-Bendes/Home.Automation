
using Microsoft.AspNetCore.SignalR.Client;

namespace Home.Automation.WebClient.Services;

public sealed class LiveUpdaterService : IAsyncDisposable
{
    private const string _hubName = "dashboard/live";

    private HubConnection? _hubConnection;

    private readonly DeviceRepository _repository;

    public LiveUpdaterService(DeviceRepository repository, IConfiguration configuration)
    {
        _repository = repository;

        var baseUrl = configuration.GetConnectionString("WebApi") ?? string.Empty;
        var hubUrl = $"{baseUrl}/{_hubName}";

        _hubConnection = new HubConnectionBuilder()
          .WithUrl(hubUrl)
          .WithAutomaticReconnect()
          .Build();

        _hubConnection.On<LiveTemperatureData>("room_temp_update", response => GroupAddedHandler?.Invoke(response));
        _hubConnection.On<LiveDoorStatusData>("door_sensor_opened", response => DoorOpenedHandler?.Invoke(response));
        _hubConnection.On<LiveDoorStatusData>("door_sensor_closed", response => DoorClosedHandler?.Invoke(response));
    }

    public event Action<LiveTemperatureData>? GroupAddedHandler;
    public event Action<LiveDoorStatusData>? DoorOpenedHandler;
    public event Action<LiveDoorStatusData>? DoorClosedHandler;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _hubConnection!.StartAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is null)
        {
            return;
        }

        await _hubConnection.StopAsync();
        await _hubConnection.DisposeAsync();
        _hubConnection = null;
    }
}

public sealed record LiveTemperatureData(
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