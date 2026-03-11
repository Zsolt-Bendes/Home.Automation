using Home.Automation.Api.Domain.TempAndHumiditySensors;
using Home.Automation.Api.Domain.TempAndHumiditySensors.Events;

namespace Home.Automation.IntegrationTests.Tests;

[ClassDataSource<AlbaBootstrap>(Shared = SharedType.PerTestSession)]
public sealed class When_measurement_arrived(AlbaBootstrap albaBootstrap) : AlbaTestBase(albaBootstrap)
{
    //[Test]
    //public async Task Given_sensor_exists_then_dashboard_is_updated()
    //{
    //    // Arrange
    //    var sensorId = await RegisterTempSensor();
    //    var measurementEvt = new TemperatureMeasurementReceived(sensorId, 15, 20, DateTimeOffset.UtcNow);

    //    using var session = Store.LightweightSession();

    //    // Act
    //    await Host.Scenario(_ =>
    //    {

    //    });

    //    var projection = new DashboardViewProjection();
    //    await projection.Project(measurementEvt, session);

    //    // Assert
    //    using var assertSession = Store.LightweightSession();

    //    var dashboard = await assertSession.LoadAsync<DashboardView>(1);

    //    await Assert.That(dashboard!.TemperatureSensors[0].TodayTemperature.Min).IsGreaterThan(0);
    //}

    private async Task<Guid> RegisterTempSensor()
    {
        using var session = Store.LightweightSession();

        var id = Guid.CreateVersion7();
        session.Events.StartStream<TemperatureAndHumiditySensor>(
            id,
            new TemperatureSensorRegistered(
                id,
                new Api.Domain.DoorSensor.ValueObjects.Label("A name")));

        await session.SaveChangesAsync();

        return id;
    }
}
