using FluentValidation;
using Home.Automation.Api.Domain.Devices.Entities;
using Home.Automation.Api.Domain.Devices.Events;
using Home.Automation.Api.Domain.Devices.ValueObjects;
using Home.Automation.Api.Features.Dashboard.View;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Attributes;
using Wolverine.Http;

namespace Home.Automation.Api.Features.Device;

public sealed record RegisterDevice(
    string Name,
    SensorType SensorType,
    DoorSensorRequest? DoorSensor)
{
    public sealed class RegisterRoomValidator : AbstractValidator<RegisterDevice>
    {
        public RegisterRoomValidator()
        {
            RuleFor(_ => _.Name)
                .NotEmpty()
                .MaximumLength(DeviceName._maxLength);
            RuleFor(_ => _.DoorSensor)
                .NotNull()
                .When(_ => _.SensorType is SensorType.Door);
        }
    }
}

public sealed record DoorSensorRequest(
    DoorStatus DoorStatus,
    bool SendStatusChangeEmails,
    TimeSpan? OpenReminderTimeSpan);

public sealed record RegisterDeviceResponse(Guid Id);

public static class RegisterDeviceEndpoint
{
    internal const string _endpoint = "/devices/register";

    public static async Task<ProblemDetails> LoadAsync(
        RegisterDevice command,
        IDocumentSession documentSession,
        CancellationToken cancellationToken)
    {
        var deviceNameExists = await documentSession.Query<DashboardView>()
            .AnyAsync(_ => _.Id == 1 && _.TemperatureSensors.Any(_ => _.Name == command.Name), cancellationToken);
        if (deviceNameExists)
        {
            return new ProblemDetails
            {
                Title = $"Device with {command.Name} name already exists",
                Status = 400,
            };
        }

        return WolverineContinue.NoProblems;
    }

    [WolverinePost(_endpoint)]
    [Transactional]
    public static RegisterDeviceResponse Post(
        RegisterDevice command,
        TimeProvider timeProvider,
        IDocumentSession documentSession)
    {
        var id = Guid.CreateVersion7();

        if (command.SensorType is SensorType.Door)
        {
            _ = documentSession.Events.StartStream(
                id,
                new DeviceRegistered(
                    id,
                    new DeviceName(command.Name),
                    SensorType.Door,
                    new List<Sensor> { Domain.Devices.Entities.DoorStatusSensor.Create(
                        Guid.NewGuid(),
                        command.DoorSensor!.DoorStatus,
                        command.DoorSensor.SendStatusChangeEmails,
                        command.DoorSensor.OpenReminderTimeSpan) },
                    timeProvider.GetLocalNow()));
        }

        if (command.SensorType is SensorType.TemperatureAndHumidity)
        {
            _ = documentSession.Events.StartStream(
                id,
                new DeviceRegistered(
                    id,
                    new DeviceName(command.Name),

                    command.SensorType,
                    new List<Sensor> { new Domain.Devices.Entities.TemperatureAndHumiditySensor(
                        Guid.NewGuid(),
                        SensorType.TemperatureAndHumidity,
                        new TemperatureAndHumidityMeasurement(0, 20)) },
                    timeProvider.GetLocalNow()));
        }

        return new RegisterDeviceResponse(id);
    }
}