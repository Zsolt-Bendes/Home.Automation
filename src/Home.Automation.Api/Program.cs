using Home.Automation.Api.Domain.Devices.Events;
using Home.Automation.Api.Domain.Devices.IntegrationMessages;
using Home.Automation.Api.Features.Device;
using Home.Automation.Api.Infrastructure;
using Home.Automation.Api.Services;
using JasperFx;
using JasperFx.Core;
using Npgsql;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.RabbitMQ;

const string localQueueName = "local";
const string integrationEventQueueName = "home_integration_events";

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(opts => opts.TimestampFormat = "yyyy.MM.dd HH:mm:ss.fff ");

builder.Host.UseWolverine(opts =>
{
    var rabbitMqConnectionString = builder.Configuration.GetConnectionString("rabbitMq");
    opts.UseRabbitMq(new Uri(rabbitMqConnectionString!))
            .DisableSystemRequestReplyQueueDeclaration()
            .DeclareQueue(integrationEventQueueName);

    opts.ListenToRabbitQueue("garagedoor", q =>
    {
        q.PurgeOnStartup = false;
        q.TimeToLive(5.Minutes());
    })
    .DefaultIncomingMessage<UpdateGarageDoorStatus>();

    opts.ListenToRabbitQueue("roomtemp", q =>
    {
        q.PurgeOnStartup = false;
        q.TimeToLive(5.Minutes());
    })
    .DefaultIncomingMessage<TemperatureMeasurement>();

    opts.PublishMessage<DoorOpened>()
       .ToLocalQueue(localQueueName);

    opts.PublishMessage<DoorClosed>()
       .ToLocalQueue(localQueueName);

    opts.PublishMessage<DoorNotClosed>()
        .ToRabbitQueue(integrationEventQueueName);

    opts.ListenToRabbitQueue(integrationEventQueueName);

    opts.Policies.OnException<ConcurrencyException>().RetryTimes(3);
    opts.Policies
        .OnException<NpgsqlException>()
        .RetryWithCooldown(
            50.Milliseconds(),
            100.Milliseconds(),
            250.Milliseconds());

    opts.Policies.AutoApplyTransactions();

    opts.UseFluentValidation();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "_myAllowSpecificOrigins",
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                      });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("_myAllowSpecificOrigins");

app.MapHub<LiveUpdater>("/dashboard/live");

app.MapWolverineEndpoints(opts =>
{
    opts.WarmUpRoutes = RouteWarmup.Eager;
    opts.UseFluentValidationProblemDetailMiddleware();
});

await app.RunJasperFxCommands(args);
