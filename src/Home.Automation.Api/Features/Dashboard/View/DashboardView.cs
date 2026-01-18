namespace Home.Automation.Api.Features.Dashboard.View;

public sealed class DashboardView
{
    public int Id { get; set; } = 1;

    public List<TemperatureAndHumiditySensorView> TemperatureSensors { get; set; } = [];

    public List<DoorStatusSensor> DoorStatusSensors { get; set; } = [];
}
