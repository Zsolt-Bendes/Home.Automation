using System.Net.Http.Json;
using System.Text.Json;

namespace Home.Automation.WebClient.Services;

public class DeviceRepository
{
    private readonly DeviceHttpClient _deviceHttpClient;

    public DeviceRepository(DeviceHttpClient deviceHttpClient)
    {
        _deviceHttpClient = deviceHttpClient;
    }

    public List<DeviceBase> Devices { get; set; } = [];
    public List<TemperatureDevice> TemperatureDevices { get; set; } = [];

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        Devices = await _deviceHttpClient.LoadDevicesAsync(cancellationToken);
    }

    public async Task LoadTempDevices(CancellationToken cancellationToken)
    {
        TemperatureDevices = await _deviceHttpClient.LoadTemperatureDevicesAsync(cancellationToken);
        Console.WriteLine(JsonSerializer.Serialize(TemperatureDevices));
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

public sealed class DeviceHttpClient
{
    private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;

    public DeviceHttpClient(HttpClient httpClient)
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
}