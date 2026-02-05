using Home.Automation.WebClient.Services;

namespace Home.Automation.WebClient.Pages;

public partial class Home : IAsyncDisposable
{
    private readonly DeviceRepository _deviceRepository;
    private readonly LiveUpdaterService _liveUpdaterService;
    private readonly ILogger<Home> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public Home(DeviceRepository deviceRepository, LiveUpdaterService liveUpdaterService, ILogger<Home> logger)
    {
        _deviceRepository = deviceRepository;
        _liveUpdaterService = liveUpdaterService;
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    protected override async Task OnInitializedAsync()
    {
        await _deviceRepository.LoadTempDevices(_cancellationTokenSource.Token);
        await _deviceRepository.LoadDashboardAsync(_cancellationTokenSource.Token);

        _logger.LogInformation(_deviceRepository.DashboardView.Id.ToString());

        _liveUpdaterService.GroupAddedHandler += UpdateTempSensor;

        await _liveUpdaterService.StartAsync(_cancellationTokenSource.Token);
        await base.OnInitializedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _liveUpdaterService.GroupAddedHandler -= UpdateTempSensor;

        _cancellationTokenSource.Cancel();
        await _liveUpdaterService.DisposeAsync();
    }

    private void UpdateTempSensor(LiveTempData data)
    {
        _logger.LogInformation(data.DeviceId.ToString());
        var device = _deviceRepository.DashboardView.TemperatureSensors.Find(_ => _.Id == data.DeviceId);
        if (device is null)
        {
            Console.WriteLine("device not found");
            return;
        }
        device.Current.Temperature = data.Temperature;

        device.Current.Humidity = data.Humidity;

        StateHasChanged();
    }
}
