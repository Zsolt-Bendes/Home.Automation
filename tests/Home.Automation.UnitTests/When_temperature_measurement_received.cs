using Home.Automation.Api.Domain.TempAndHumiditySensors;
using Home.Automation.Api.Domain.TempAndHumiditySensors.Events;
using Home.Automation.Api.Features.Dashboard;
using Home.Automation.Api.Features.Dashboard.View;
using Home.Automation.UnitTests.Builders;
using Marten;
using NSubstitute;

namespace Home.Automation.UnitTests;

public sealed class When_temperature_measurement_received
{
    private static readonly TemperatureAndHumiditySensorBuilder _sensorBuilder = new();

    [Test]
    public void Given_sensor_does_not_exists_then_measurements_are_not_changed()
    {
        // Arrange
        var sensor = _sensorBuilder.Build();

        var evt = new TemperatureMeasurementReceived(Guid.NewGuid(), 0, 0, DateTimeOffset.UtcNow);

        // Act
        TemperatureAndHumiditySensor.Apply(evt, sensor);

        // Assert
        Assert.That(sensor!.Measurement).IsNull();
        Assert.That(sensor!.MeasurementCount).IsEqualTo(0);
    }

    [Test]
    public void Given_sensor_does_exists_then_measurements_daily_counter_is_incremented()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var sensor = _sensorBuilder
            .WithId(sensorId)
            .Build();

        var evt = new TemperatureMeasurementReceived(sensorId, 0, 0, DateTimeOffset.UtcNow);

        // Act
        TemperatureAndHumiditySensor.Apply(evt, sensor);

        // Assert
        Assert.That(sensor!.MeasurementCount).IsEqualTo(1);
    }

    [Test]
    public void Given_sensor_does_exists_then_measurements_are_set()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var sensor = _sensorBuilder
            .WithId(sensorId)
            .Build();

        var evt = new TemperatureMeasurementReceived(sensorId, 0, 0, DateTimeOffset.UtcNow);

        // Act
        TemperatureAndHumiditySensor.Apply(evt, sensor);

        // Assert
        Assert.That(sensor!.Measurement).IsNotNull();
    }

    [Test]
    public void Given_sensor_does_exists_then_average_is_calculated()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var sensor = _sensorBuilder
            .WithId(sensorId)
            .Build();

        var evt = new TemperatureMeasurementReceived(sensorId, 21, 60, DateTimeOffset.UtcNow);

        // Act
        TemperatureAndHumiditySensor.Apply(evt, sensor);

        // Assert
        Assert.That(sensor!.AverageTemperature).IsGreaterThan(0);
        Assert.That(sensor!.AverageHumidity).IsGreaterThan(0);
    }

    [Test]
    public void Given_sensor_does_exists_and_second_measurement_is_next_day_then_counter_is_reset()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var sensor = _sensorBuilder
            .WithId(sensorId)
            .Build();

        var evt1 = new TemperatureMeasurementReceived(sensorId, 21, 60, DateTimeOffset.UtcNow);
        var evt2 = new TemperatureMeasurementReceived(sensorId, 21, 60, DateTimeOffset.UtcNow.AddDays(1));

        // Act
        TemperatureAndHumiditySensor.Apply(evt1, sensor);
        TemperatureAndHumiditySensor.Apply(evt2, sensor);

        // Assert
        Assert.That(sensor!.MeasurementCount).IsEqualTo(1);
    }

    [Test]
    public void Given_measurement_max_values_are_greater_then_max_values_are_updated()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var sensor = _sensorBuilder
            .WithId(sensorId)
            .Build();

        var evt1 = new TemperatureMeasurementReceived(sensorId, 20, 50, DateTimeOffset.UtcNow);
        var evt2 = new TemperatureMeasurementReceived(sensorId, 21, 60, DateTimeOffset.UtcNow);

        // Act
        TemperatureAndHumiditySensor.Apply(evt1, sensor);
        TemperatureAndHumiditySensor.Apply(evt2, sensor);

        // Assert
        Assert.That(sensor!.MaxTemperature).IsEqualTo(21);
        Assert.That(sensor!.MaxHumidity).IsEqualTo(60);
    }

    [Test]
    public void Given_measurement_min_values_are_lower_then_min_values_are_updated()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var sensor = _sensorBuilder
            .WithId(sensorId)
            .Build();

        var evt1 = new TemperatureMeasurementReceived(sensorId, 21, 60, DateTimeOffset.UtcNow);
        var evt2 = new TemperatureMeasurementReceived(sensorId, 20, 50, DateTimeOffset.UtcNow);

        // Act
        TemperatureAndHumiditySensor.Apply(evt1, sensor);
        TemperatureAndHumiditySensor.Apply(evt2, sensor);

        // Assert
        Assert.That(sensor!.MinTemperature).IsEqualTo(20);
        Assert.That(sensor!.MinHumidity).IsEqualTo(50);
    }

    [Test]
    public void Given_2nd_measurement_arrived_then_average_calculations_are_updated()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var sensor = _sensorBuilder
            .WithId(sensorId)
            .Build();

        var evt1 = new TemperatureMeasurementReceived(sensorId, 21, 60, DateTimeOffset.UtcNow);
        var evt2 = new TemperatureMeasurementReceived(sensorId, 20, 50, DateTimeOffset.UtcNow);

        // Act
        TemperatureAndHumiditySensor.Apply(evt1, sensor);
        TemperatureAndHumiditySensor.Apply(evt2, sensor);

        // Assert
        Assert.That(sensor!.AverageTemperature).IsEqualTo(evt1.TemperatureInCelsius + evt2.TemperatureInCelsius / 2);
        Assert.That(sensor!.AverageHumidity).IsEqualTo(evt1.Humidity + evt2.Humidity / 2);
    }

    [Test]
    public async Task Given_sensor_exists_then_dashboard_is_updated()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var measurementEvt = new TemperatureMeasurementReceived(sensorId, 15, 20, DateTimeOffset.UtcNow);

        var dashboardView = new DashboardView();
        dashboardView.TemperatureSensors.Add(new TemperatureAndHumiditySensorView(sensorId, "a name"));

        IDocumentOperations ops = Substitute.For<IDocumentOperations>();
        ops.LoadAsync<DashboardView>(1, CancellationToken.None).Returns(await Task.FromResult(dashboardView));

        // Act
        var projection = new DashboardViewProjection();
        await projection.Project(measurementEvt, ops);

        // Assert
        await Assert.That(dashboardView.TemperatureSensors[0].TodayTemperature.Min).IsGreaterThan(0);
    }
}
