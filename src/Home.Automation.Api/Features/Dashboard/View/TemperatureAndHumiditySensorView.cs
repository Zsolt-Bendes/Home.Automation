using Home.Automation.Api.Domain.Devices.ValueObjects;

namespace Home.Automation.Api.Features.Dashboard.View;

public sealed class TemperatureAndHumiditySensorView : SensorsViewBase
{
    public TemperatureAndHumiditySensorView(Guid id, string name)
        : base(id, name)
    {
    }

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
