using Home.Automation.Api.Domain.Devices.ValueObjects;

namespace Home.Automation.Api.Features.Dashboard.View;

public sealed class TemperatureAndHumiditySensorView : SensorsViewBase
{
    public TemperatureAndHumiditySensorView(Guid id, string name)
        : base(id, name)
    {
    }

    public TemperatureAndHumidityMeasurement? Current { get; set; }
}
