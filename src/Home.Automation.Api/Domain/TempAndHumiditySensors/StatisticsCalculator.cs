using Home.Automation.Api.Domain.TempAndHumiditySensors.Events;

namespace Home.Automation.Api.Domain.TempAndHumiditySensors;

public static class StatisticsCalculator
{
    public static TemperatureMeasurementStatisticsUpdated CalculateStatistics(
        DateTimeOffset? prevMeasurementDate,
        int prevMeasurementCount,
        double minTemperature,
        double maxTemperature,
        double sumOfTemperature,
        double minHumidity,
        double maxHumidity,
        double sumOfHumidity,
        TemperatureMeasurementReceived newMeasurement)
    {
        if (prevMeasurementDate?.Day == newMeasurement.MeasuredAt.Day)
        {
            prevMeasurementCount++;

            if (newMeasurement.Humidity > 0)
            {
                sumOfHumidity += newMeasurement.Humidity;
            }

            if (newMeasurement.TemperatureInCelsius > 0)
            {
                sumOfTemperature += newMeasurement.TemperatureInCelsius;
            }

            return new TemperatureMeasurementStatisticsUpdated(
                newMeasurement.SensorId,
                prevMeasurementCount,
                sumOfTemperature / prevMeasurementCount,
                sumOfHumidity / prevMeasurementCount,
                minTemperature > newMeasurement.TemperatureInCelsius ? newMeasurement.TemperatureInCelsius : minTemperature,
                minHumidity > newMeasurement.Humidity ? newMeasurement.Humidity : minHumidity,
                maxTemperature < newMeasurement.TemperatureInCelsius ? newMeasurement.TemperatureInCelsius : maxTemperature,
                maxHumidity < newMeasurement.Humidity ? newMeasurement.Humidity : maxHumidity,
                sumOfTemperature,
                sumOfHumidity);
        }

        return new TemperatureMeasurementStatisticsUpdated(
            newMeasurement.SensorId,
            1,
            newMeasurement.TemperatureInCelsius,
            newMeasurement.Humidity,
            newMeasurement.TemperatureInCelsius,
            newMeasurement.Humidity,
            newMeasurement.TemperatureInCelsius,
            newMeasurement.Humidity,
            newMeasurement.TemperatureInCelsius,
            newMeasurement.Humidity);
    }
}
