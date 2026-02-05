using System.Net.Http.Json;
using System.Text.Json;

namespace Home.Automation.WebClient.Services;

public class DeviceRepository
{
    private readonly SensorHttpClient _deviceHttpClient;

    public DeviceRepository(SensorHttpClient deviceHttpClient)
    {
        _deviceHttpClient = deviceHttpClient;
    }

    public List<DeviceBase> Devices { get; set; } = [];
    public List<TemperatureDevice> TemperatureDevices { get; set; } = [];
    public DashboardView? DashboardView { get; set; }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        Devices = await _deviceHttpClient.LoadDevicesAsync(cancellationToken);
    }

    public async Task LoadTempDevices(CancellationToken cancellationToken)
    {
        TemperatureDevices = await _deviceHttpClient.LoadTemperatureDevicesAsync(cancellationToken);
    }

    public async Task LoadDashboardAsync(CancellationToken cancellationToken)
    {
        DashboardView = await _deviceHttpClient.LoadDashboardAsync(1, cancellationToken);
    }

    public async Task<TemperatureDevice?> GetDeviceAsync(Guid id, CancellationToken cancellationToken)
    {
        if (TemperatureDevices is null)
        {
            TemperatureDevices = await _deviceHttpClient.LoadTemperatureDevicesAsync(cancellationToken);
        }

        return TemperatureDevices.Find(_ => _.Id == id);
    }
}

public abstract class DeviceBase
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class TemperatureDevice : DeviceBase
{
    public double Humidity { get; set; }
    public double Temperature { get; set; }
}

public sealed class SensorHttpClient
{
    private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;

    public SensorHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<DeviceBase>> LoadDevicesAsync(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetFromJsonAsync<List<DeviceBase>>("/devices", _serializerOptions, cancellationToken);
        Console.WriteLine(JsonSerializer.Serialize(result));
        return result ?? [];
    }

    public async Task<List<TemperatureDevice>> LoadTemperatureDevicesAsync(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetFromJsonAsync<List<TemperatureDevice>>("/devices/temperature_sensors", _serializerOptions, cancellationToken);
        Console.WriteLine(JsonSerializer.Serialize(result));
        return result ?? [];
    }

    public async Task<DashboardView?> LoadDashboardAsync(int i = 1, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<DashboardView>("/dashboard", _serializerOptions, cancellationToken);
    }
}

public sealed class DoorStatusSensor : DeviceBase
{
    public DoorStatus DoorStatus { get; set; }

    public bool SendReminderEmail { get; set; }
    public TimeSpan? ReminderTimeSpan { get; }
    public DateTimeOffset? OpenedAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }
}

public enum DoorStatus
{
    Open = 0,
    Closed = 1,
}

public sealed class TemperatureAndHumiditySensorView : DeviceBase
{
    public TemperatureAndHumidityMeasurement? Current { get; set; }

    public MinMaxAverage TodayTemperature { get; set; }

    public MinMaxAverage TodayHumidity { get; set; }

    public int DailyMeasurementCount { get; set; }

    public double SumOfHumidity { get; set; }
    public double SumOfTemperature { get; set; }
}

public struct MinMaxAverage
{
    public MinMaxAverage(double min, double max, double average)
    {
        Min = min;
        Max = max;
        Average = average;
    }

    public double Min { get; set; }

    public double Max { get; set; }

    public double Average { get; set; }
}

public sealed class TemperatureAndHumidityMeasurement
{
    public double Temperature { get; set; }

    public double Humidity { get; set; }
}

public sealed class DashboardView
{
    public int Id { get; set; }

    public List<TemperatureAndHumiditySensorView> TemperatureSensors { get; set; } = [];

    public List<DoorStatusSensor> DoorStatusSensors { get; set; } = [];
}