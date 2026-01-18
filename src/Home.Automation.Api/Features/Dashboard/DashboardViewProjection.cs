using Home.Automation.Api.Domain.Devices.Events;
using Home.Automation.Api.Domain.Devices.ValueObjects;
using Home.Automation.Api.Features.Dashboard.View;
using Marten;
using Marten.Events.Projections;

namespace Home.Automation.Api.Features.Dashboard;

public sealed class DashboardViewProjection : EventProjection
{
    public async Task Project(DeviceRegistered evt, IDocumentOperations ops)
    {
        var view = await ops.LoadAsync<DashboardView>(1);
        view ??= new DashboardView();

        foreach (var sensor in evt.Sensors)
        {
            if (sensor.Type is Domain.Devices.ValueObjects.SensorType.TemperatureAndHumidity)
            {
                view.TemperatureSensors.Add(new TemperatureAndHumiditySensorView(evt.Id, evt.Name.Name));
            }

            if (sensor.Type is Domain.Devices.ValueObjects.SensorType.Door)
            {
                var doorSensor = sensor as Domain.Devices.Entities.DoorStatusSensor;
                view.DoorStatusSensors.Add(new DoorStatusSensor(
                  evt.Id,
                  evt.Name.Name,
                  doorSensor!.DoorStatus,
                  null,
                  null));
            }
        }

        ops.Store(view);
    }

    public async Task Project(TemperatureMeasurementReceived evt, IDocumentOperations ops)
    {
        var view = await ops.LoadAsync<DashboardView>(1);
        view ??= new DashboardView();

        var sensor = view.TemperatureSensors.Find(_ => _.Id == evt.DeviceId);
        if (sensor is null)
        {
            return;
        }

        sensor.Current = new TemperatureAndHumidityMeasurement(evt.TemperatureInCelsius, evt.Humidity);

        ops.Store(view);
    }

    public async Task Project(DoorOpened evt, IDocumentOperations ops)
    {
        var dashboard = await ops.LoadAsync<DashboardView>(1);
        if (dashboard is null)
        {
            return;
        }

        var device = dashboard.DoorStatusSensors.Find(_ => _.Id == evt.DeviceId);
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

        var device = dashboard.DoorStatusSensors.Find(_ => _.Id == evt.DeviceId);
        if (device is null)
        {
            return;
        }

        device.DoorStatus = DoorStatus.Closed;
        device.ClosedAt = evt.HappenedAt;

        ops.Store(dashboard);
    }
}