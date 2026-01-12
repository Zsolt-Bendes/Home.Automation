using Home.Automation.Api.Domain.Garages;
using Home.Automation.Api.Infrastructure.Projections;
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

        Projections.LiveStreamAggregation<Garage>();

        Projections.Add<GarageViewProjection>(ProjectionLifecycle.Inline);
        Projections.Add<DashboardViewProjection>(ProjectionLifecycle.Inline);
    }
}
