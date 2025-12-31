using Home.Automation.Api.Domain.Garages.Events;

namespace Home.Automation.Api.Domain.Garages;

public sealed record Garage(Guid Id, GarageDoorStatus DoorStatus)
{
    public static Garage Create(GarageRegistered registerGarage) =>
        new Garage(registerGarage.GarageId, registerGarage.DoorStatus);

    public static Garage Apply(GarageDoorClosed garageDoorClosed, Garage garage)
    {
        if (garage.DoorStatus is not GarageDoorStatus.Open)
        {
            return garage;
        }

        return garage with { DoorStatus = GarageDoorStatus.Closed };
    }

    public static Garage Apply(GarageDoorOpened garageDoorClosed, Garage garage)
    {
        if (garage.DoorStatus is not GarageDoorStatus.Closed)
        {
            return garage;
        }

        return garage with { DoorStatus = GarageDoorStatus.Open };
    }
}