using Home.Automation.Api.Domain.Garages;
using Home.Automation.Api.Domain.Garages.Events;
using Marten;
using Wolverine.Attributes;

namespace Home.Automation.Api.Features.Garage;

public sealed record RegisterGarage(Guid GarageId, GarageDoorStatus DoorStatus);

public static class RegisterGarageHandler
{
    public static async Task<bool> LoadAsync(
        RegisterGarage command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        var garage = await session.LoadAsync<GarageView>(command.GarageId, cancellationToken);
        return garage is not null;
    }

    [Transactional]
    public static GarageRegistered Handle(
        RegisterGarage command,
        bool isRegistered,
        IDocumentSession session,
        TimeProvider timeProvider)
    {
        if (isRegistered)
        {
            return null!;
        }

        var evt = new GarageRegistered(command.GarageId, command.DoorStatus, timeProvider.GetLocalNow());
        session.Events.StartStream<Domain.Garages.Garage>(command.GarageId, evt);

        return evt;
    }
}