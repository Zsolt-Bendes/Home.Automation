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
        await _deviceRepository.LoadDashboardAsync(_cancellationTokenSource.Token);

        _liveUpdaterService.GroupAddedHandler += UpdateTempSensor;
        _liveUpdaterService.DoorOpenedHandler += DoorOpened;
        _liveUpdaterService.DoorClosedHandler += DoorClosed;

        await _liveUpdaterService.StartAsync(_cancellationTokenSource.Token);
        await base.OnInitializedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _liveUpdaterService.GroupAddedHandler -= UpdateTempSensor;
        _liveUpdaterService.DoorOpenedHandler -= DoorOpened;
        _liveUpdaterService.DoorClosedHandler -= DoorClosed;

        _cancellationTokenSource.Cancel();
        await _liveUpdaterService.DisposeAsync();
    }

    private async void UpdateTempSensor(LiveTemperatureData data)
    {
        var sensor = _deviceRepository.DashboardView!.TemperatureSensors.Find(_ => _.Id == data.SensorId);
        if (sensor is null)
        {
            return;
        }

        sensor.Current!.Temperature = data.Temperature;
        sensor.Current.Humidity = data.Humidity;

        sensor.TodayTemperature = new MinMaxAverage(data.MinTemp, data.MaxTemp, data.AvgTemp);
        sensor.TodayHumidity = new MinMaxAverage(data.MinHumidity, data.MaxHumidity, data.AvgHumidity);

        sensor.PastState = await _deviceRepository.GetAggregatedStatsAsync(sensor.GraphView, sensor.Id, _cancellationTokenSource.Token);

        StateHasChanged();
    }

    private void DoorOpened(LiveDoorStatusData data)
    {
        var sensor = _deviceRepository.DashboardView!.DoorStatusSensors.Find(_ => _.Id == data.SensorId);
        if (sensor is null)
        {
            return;
        }

        sensor.DoorStatus = data.DoorStatus;
        sensor.OpenedAt = data.HappenedAt;

        StateHasChanged();
    }

    private void DoorClosed(LiveDoorStatusData data)
    {
        var sensor = _deviceRepository.DashboardView!.DoorStatusSensors.Find(_ => _.Id == data.SensorId);
        if (sensor is null)
        {
            return;
        }

        sensor.DoorStatus = data.DoorStatus;
        sensor.ClosedAt = data.HappenedAt;

        StateHasChanged();
    }

    private async Task OnFilterChangedAsync(TemperatureAndHumiditySensorView? sensor)
    {
        if (sensor is null)
        {
            return;
        }

        sensor.PastState = await _deviceRepository.GetAggregatedStatsAsync(
            sensor.GraphView,
            sensor.Id,
            _cancellationTokenSource.Token);
    }

    private string GetXAxisDataSource(TemperatureAndHumiditySensorView? sensor)
    {
        if (sensor is null)
        {
            return "Hours";
        }

        return sensor.GraphView switch
        {
            Filter.Last24Hours => "Hours",
            Filter.PrevWeek => "DateTime",
            Filter.PrevMonth => "DateTime",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
