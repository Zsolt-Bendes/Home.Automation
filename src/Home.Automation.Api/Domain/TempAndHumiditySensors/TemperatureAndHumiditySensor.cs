using Home.Automation.Api.Domain.Devices.ValueObjects;
using Home.Automation.Api.Domain.TempAndHumiditySensors.Events;

namespace Home.Automation.Api.Domain.TempAndHumiditySensors;

public sealed record TemperatureAndHumiditySensor(
    Guid Id,
    TemperatureAndHumidityMeasurement? measurement)
{
    public TemperatureAndHumidityMeasurement? Measurement { get; set; }

    public int MeasurementCount { get; private set; }
    public double SumOfTemperature { get; private set; }
    public double SumOfHumidity { get; private set; }

    public double AverageTemperature { get; private set; }
    public double AverageHumidity { get; private set; }

    public double MinTemperature { get; private set; }
    public double MinHumidity { get; private set; }
    public double MaxTemperature { get; private set; }
    public double MaxHumidity { get; private set; }

    public static TemperatureAndHumiditySensor Create(TemperatureSensorRegistered temperatureSensorRegistered) =>
        new TemperatureAndHumiditySensor(temperatureSensorRegistered.SensorId, null);

    public static void Apply(TemperatureMeasurementReceived newMeasurement, TemperatureAndHumiditySensor sensor)
    {
        var result = StatisticsCalculator.CalculateStatistics(
            sensor.Measurement?.MeasuredAt,
            sensor.MeasurementCount,
            sensor.MinTemperature,
            sensor.MaxTemperature,
            sensor.SumOfTemperature,
            sensor.MinHumidity,
            sensor.MaxHumidity,
            sensor.SumOfHumidity,
            newMeasurement);

        sensor.MinTemperature = result.MinTemperature;
        sensor.MaxTemperature = result.MaxTemperature;
        sensor.AverageTemperature = result.AverageTemperature;
        sensor.MinHumidity = result.MinHumidity;
        sensor.MaxHumidity = result.MaxHumidity;
        sensor.AverageHumidity = result.AverageHumidity;

        sensor.MeasurementCount = result.MeasurementCounter;
        sensor.SumOfTemperature = result.DailySumOfTemperature;
        sensor.SumOfHumidity = result.DailySumOfHumidity;
    }
}

public sealed record TemperatureMeasurementStatisticsUpdated(
    Guid SensorId,
    int MeasurementCounter,
    double AverageTemperature,
    double AverageHumidity,
    double MinTemperature,
    double MinHumidity,
    double MaxTemperature,
    double MaxHumidity,
    double DailySumOfTemperature,
    double DailySumOfHumidity);