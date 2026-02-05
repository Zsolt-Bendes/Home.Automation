using FluentValidation;
using Home.Automation.Api.Domain.Devices.Events;
using Home.Automation.Api.Domain.Devices.ValueObjects;
using Home.Automation.Api.Domain.DoorSensor.ValueObjects;
using Home.Automation.Api.Domain.TempAndHumiditySensors.Events;
using Home.Automation.Api.Features.Dashboard.View;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Attributes;
using Wolverine.Http;

namespace Home.Automation.Api.Features.Device;

public sealed record RegisterDevice(
    string Label,
    SensorType SensorType,
    DoorSensorRequest? DoorSensor)
{
    public sealed class RegisterRoomValidator : AbstractValidator<RegisterDevice>
    {
        public RegisterRoomValidator()
        {
            RuleFor(_ => _.Label)
                .NotEmpty()
                .MaximumLength(Domain.DoorSensor.ValueObjects.Label._maxLength);
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
            .AnyAsync(_ => _.Id == 1 && _.TemperatureSensors.Any(_ => _.Name == command.Label), cancellationToken);
        if (deviceNameExists)
        {
            return new ProblemDetails
            {
                Title = $"Sensor with {command.Label} label already exists",
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
                new DoorSensorRegistered(
                    id,
                    new Label(command.Label),
                    command.DoorSensor!.DoorStatus,
                    command.DoorSensor.SendStatusChangeEmails,
                    command.DoorSensor.OpenReminderTimeSpan));
        }

        if (command.SensorType is SensorType.TemperatureAndHumidity)
        {
            _ = documentSession.Events.StartStream(
                id,
                new TemperatureSensorRegistered(
                    id,
                    new Label(command.Label)));
        }

        return new RegisterDeviceResponse(id);
    }
}