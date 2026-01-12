using Home.Automation.Api.Domain.Garages;
using Home.Automation.Api.Domain.Garages.Events;
using Home.Automation.Api.Domain.Rooms.Events;
using Home.Automation.Api.Features.Dashboard;
using Home.Automation.Api.Features.Garage;
using Marten;
using Marten.Events.Projections;

namespace Home.Automation.Api.Infrastructure.Projections;

public sealed class GarageViewProjection : EventProjection
{
    public async Task Project(GarageRegistered evt, IDocumentOperations ops)
    {
        var view = await ops.LoadAsync<GarageView>(evt.GarageId);
        if (view is not null)
        {
            return;
        }

        view = new GarageView(evt.GarageId, evt.DoorStatus, evt.HappenedAt);
        await UpdateDashboardAsync(evt.DoorStatus, ops);

        ops.Store(view);
    }

    public async Task Project(GarageDoorOpened evt, IDocumentOperations ops)
    {
        var view = await ops.LoadAsync<GarageView>(evt.GarageId);
        if (view is null)
        {
            return;
        }

        view.DoorStatus = GarageDoorStatus.Open;
        view.OpenedAt = evt.HappenedAt;

        await UpdateDashboardAsync(GarageDoorStatus.Open, ops);

        ops.Store(view);
    }

    public async Task Project(GarageDoorClosed evt, IDocumentOperations ops)
    {
        var view = await ops.LoadAsync<GarageView>(evt.GarageId);
        if (view is null)
        {
            return;
        }

        view.DoorStatus = GarageDoorStatus.Closed;
        view.ClosedAt = evt.HappenedAt;
        await UpdateDashboardAsync(GarageDoorStatus.Closed, ops);

        ops.Store(view);
    }

    private static async Task UpdateDashboardAsync(GarageDoorStatus garageDoorStatus, IDocumentOperations ops)
    {
        var dashboard = await ops.LoadAsync<DashboardView>(1);
        if (dashboard is null)
        {
            return;
        }

        dashboard.GarageDoorStatus = garageDoorStatus;

        ops.Store(dashboard);
    }
}

public sealed class DashboardViewProjection : EventProjection
{
    public async Task Project(RoomRegistered evt, IDocumentOperations ops)
    {
        var view = await ops.LoadAsync<DashboardView>(1);
        if (view is null)
        {
            view = new DashboardView();
        }

        view.Rooms.Add(new RoomView()
        {
            Id = evt.Id,
            Name = evt.Name.Name
        });

        ops.Store(view);
    }

    public async Task Project(TemperatureMeasurementReceived evt, IDocumentOperations ops)
    {
        var view = await ops.LoadAsync<DashboardView>(1);
        if (view is null)
        {
            view = new DashboardView();
        }

        var room = view.Rooms.Find(_ => _.Id == evt.RoomId);
        if (room is null)
        {
            return;
        }

        room.Current = new TempSensorData()
        {
            Humidity = evt.Humidity,
            Temperature = evt.TemperatureInCelsius,
        };

        ops.Store(view);
    }
}