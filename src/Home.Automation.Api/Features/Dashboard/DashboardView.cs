using Home.Automation.Api.Domain.Garages;

namespace Home.Automation.Api.Features.Dashboard;

public sealed class DashboardView
{
    public int Id { get; set; } = 1;
    public List<RoomView> Rooms { get; set; } = [];
    public GarageDoorStatus GarageDoorStatus { get; set; }
}

public sealed class RoomView
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public TempSensorData? Current { get; set; }
}

public sealed class TempSensorData
{
    public double Temperature { get; set; }
    public double Humidity { get; set; }
}
