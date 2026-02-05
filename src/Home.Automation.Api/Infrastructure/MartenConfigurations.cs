using Home.Automation.Api.Domain.DoorSensor;
using Home.Automation.Api.Domain.TempAndHumiditySensors;
using Home.Automation.Api.Features.Dashboard;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;

namespace Home.Automation.Api.Infrastructure;

public sealed class MartenConfigurations : StoreOptions
{
    public MartenConfigurations(string connectionString)
    {
        DisableNpgsqlLogging = true;
        Connection(connectionString);
        UseSystemTextJsonForSerialization();

        Events.UseArchivedStreamPartitioning = true;
        Events.UseIdentityMapForAggregates = true;
        Events.AppendMode = EventAppendMode.Quick;

        Projections.LiveStreamAggregation<DoorStatusSensor>();
        Projections.LiveStreamAggregation<TemperatureAndHumiditySensor>();

        Projections.Add<DashboardViewProjection>(ProjectionLifecycle.Inline);
    }
}
