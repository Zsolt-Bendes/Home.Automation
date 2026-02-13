using Home.Automation.Api.Features.Dashboard.View;
using Marten;
using Wolverine.Http;

namespace Home.Automation.Api.Features.Device;

public sealed record TemperatureAndHumiditySensorResponse(
    Guid Id,
    string Name,
    double Humidity,
    double Temperature);

public static class LoadDevicesEndpoint
{
    internal const string _path = "/devices/temperature_sensors";

    [WolverineGet(_path)]
    public static async Task<List<TemperatureAndHumiditySensorResponse>> Get(IQuerySession querySession, CancellationToken cancellationToken)
    {
        var tmp = await querySession.Query<DashboardView>()
            .SelectMany(_ => _.TemperatureSensors)
            .ToListAsync(cancellationToken);

        return [.. tmp.Select(_ => new TemperatureAndHumiditySensorResponse(
            _.Id,
            _.Name,
            _.Current?.Humidity ?? 0,
            _.Current?.Temperature ?? 0)
        )];
    }
}