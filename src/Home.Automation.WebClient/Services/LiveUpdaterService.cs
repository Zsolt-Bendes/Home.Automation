
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

        _hubConnection.On<LiveTempData>("room_temp_update", response => GroupAddedHandler?.Invoke(response));
    }

    public event Action<LiveTempData>? GroupAddedHandler;

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

public sealed record LiveTempData(Guid DeviceId, double Temperature, double Humidity);