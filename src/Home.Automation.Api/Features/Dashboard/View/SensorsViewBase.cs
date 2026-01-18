namespace Home.Automation.Api.Features.Dashboard.View;

public abstract class SensorsViewBase
{
    protected SensorsViewBase(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}