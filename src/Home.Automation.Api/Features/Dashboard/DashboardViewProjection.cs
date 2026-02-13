using Home.Automation.Api.Domain.Devices.Events;
using Home.Automation.Api.Domain.Devices.ValueObjects;
using Home.Automation.Api.Domain.TempAndHumiditySensors;
using Home.Automation.Api.Domain.TempAndHumiditySensors.Events;
using Home.Automation.Api.Features.Dashboard.View;
using Marten;
using Marten.Events.Projections;

namespace Home.Automation.Api.Features.Dashboard;

public sealed class DashboardViewProjection : EventProjection
{
    public async Task Project(DoorSensorRegistered evt, IDocumentOperations ops)
    {
        var view = await ops.LoadAsync<DashboardView>(1);
        view ??= new DashboardView();

        view.DoorStatusSensors.Add(new DoorStatusSensor(
          evt.Id,
          evt.Label.Name,
          evt.DoorStatus,
          evt.SendNotification,
          evt.OpenReminderTimeSpan,
          null,
          null));

        ops.Store(view);
    }

    public async Task Project(TemperatureSensorRegistered evt, IDocumentOperations ops)
    {
        var view = await ops.LoadAsync<DashboardView>(1);
        view ??= new DashboardView();

        view.TemperatureSensors.Add(new TemperatureAndHumiditySensorView(evt.SensorId, evt.Name.Name));

        ops.Store(view);
    }

    public async Task Project(TemperatureMeasurementReceived evt, IDocumentOperations ops)
    {
        var view = await ops.LoadAsync<DashboardView>(1);
        view ??= new DashboardView();

        var sensor = view.TemperatureSensors.Find(_ => _.Id == evt.SensorId);
        if (sensor is null)
        {
            return;
        }

        var statistics = StatisticsCalculator.CalculateStatistics(
            sensor.Current?.MeasuredAt,
            sensor.DailyMeasurementCount,
            sensor.TodayTemperature.Min,
            sensor.TodayTemperature.Max,
            sensor.SumOfTemperature,
            sensor.TodayHumidity.Min,
            sensor.TodayHumidity.Max,
            sensor.SumOfHumidity,
            evt);

        sensor.Current = new TemperatureAndHumidityMeasurement(
          evt.TemperatureInCelsius,
          evt.Humidity,
          evt.MeasuredAt);

        sensor.DailyMeasurementCount = statistics.MeasurementCounter;
        sensor.SumOfHumidity = statistics.DailySumOfHumidity;
        sensor.SumOfTemperature = statistics.DailySumOfTemperature;

        sensor.TodayHumidity = new MinMaxAverage(statistics.MinHumidity, statistics.MaxHumidity, statistics.AverageHumidity);
        sensor.TodayTemperature = new MinMaxAverage(statistics.MinTemperature, statistics.MaxTemperature, statistics.AverageTemperature);

        ops.Store(view);
    }

    public async Task Project(DoorOpened evt, IDocumentOperations ops)
    {
        var dashboard = await ops.LoadAsync<DashboardView>(1);
        if (dashboard is null)
        {
            return;
        }

        var device = dashboard.DoorStatusSensors.Find(_ => _.Id == evt.SensorId);
        if (device is null)
        {
            return;
        }

        device.DoorStatus = DoorStatus.Open;
        device.OpenedAt = evt.HappenedAt;

        ops.Store(dashboard);
    }

    public async Task Project(DoorClosed evt, IDocumentOperations ops)
    {
        var dashboard = await ops.LoadAsync<DashboardView>(1);
        if (dashboard is null)
        {
            return;
        }

        var device = dashboard.DoorStatusSensors.Find(_ => _.Id == evt.SensorId);
        if (device is null)
        {
            return;
        }

        device.DoorStatus = DoorStatus.Closed;
        device.ClosedAt = evt.HappenedAt;

        ops.Store(dashboard);
    }
}