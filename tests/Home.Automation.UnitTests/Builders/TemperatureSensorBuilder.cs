//using Home.Automation.Api.Domain.Devices;
//using Home.Automation.Api.Domain.Devices.Entities;
//using Home.Automation.Api.Domain.Devices.Events;
//using Home.Automation.Api.Domain.Devices.ValueObjects;

//namespace Home.Automation.UnitTests.Builders;

//internal sealed class TemperatureSensorBuilder
//{
//    private Guid _id = Guid.CreateVersion7();
//    private string _name = "device 1";
//    private List<Sensor> _sensors = [];
//    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;

//    private readonly TemperatureAndHumiditySensorBuilder _temperatureAndHumiditySensorBuilder = new();

//    public Device Build() => Device.Create(new DeviceRegistered(_id, new DeviceName(_name), _sensors, _createdAt));

//    public TemperatureSensorBuilder WithId(Guid id)
//    {
//        _id = id;
//        return this;
//    }

//    public TemperatureSensorBuilder WithName(string name)
//    {
//        _name = name;
//        return this;
//    }

//    public TemperatureSensorBuilder WithTempSensor(params Action<TemperatureAndHumiditySensorBuilder>[] actions)
//    {
//        foreach (var action in actions)
//        {
//            action.Invoke(_temperatureAndHumiditySensorBuilder);
//            _sensors.Add(_temperatureAndHumiditySensorBuilder.Build());
//        }

//        return this;
//    }
//}
