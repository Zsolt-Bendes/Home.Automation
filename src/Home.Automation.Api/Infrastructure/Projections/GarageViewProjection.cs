using Home.Automation.Api.Domain.Garages.Events;
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
        ops.Store(view);
    }

    public async Task Project(GarageDoorOpened evt, IDocumentOperations ops)
    {
        var view = await ops.LoadAsync<GarageView>(evt.GarageId);
        if (view is null)
        {
            return;
        }

        view.DoorStatus = Domain.Garages.GarageDoorStatus.Open;
        view.OpenedAt = evt.HappenedAt;

        ops.Store(view);
    }

    public async Task Project(GarageDoorClosed evt, IDocumentOperations ops)
    {
        var view = await ops.LoadAsync<GarageView>(evt.GarageId);
        if (view is null)
        {
            return;
        }

        view.DoorStatus = Domain.Garages.GarageDoorStatus.Closed;
        view.ClosedAt = evt.HappenedAt;

        ops.Store(view);
    }
}
