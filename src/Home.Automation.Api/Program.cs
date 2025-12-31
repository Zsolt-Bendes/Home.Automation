using Home.Automation.Api.Domain.Garages.Events;
using Home.Automation.Api.Features.Garage;
using Home.Automation.Api.Infrastructure;
using JasperFx;
using JasperFx.Core;
using Npgsql;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWolverine(opts =>
{
    var rabbitMqConnectionString = builder.Configuration.GetConnectionString("rabbitMq");
    opts.UseRabbitMq(new Uri(rabbitMqConnectionString!))
            .DisableSystemRequestReplyQueueDeclaration();

    opts.ListenToRabbitQueue("garagedoor", q =>
    {
        q.PurgeOnStartup = false;
        q.TimeToLive(5.Minutes());
    })
    .DefaultIncomingMessage<UpdateGarageDoorStatus>();

    opts.ListenToRabbitQueue("garageregister", q =>
    {
        q.PurgeOnStartup = false;
        q.TimeToLive(5.Minutes());
    })
  .DefaultIncomingMessage<RegisterGarage>();

    opts.PublishMessage<GarageDoorOpened>()
       .ToLocalQueue("local");

    opts.PublishMessage<GarageDoorClosed>()
       .ToLocalQueue("local");

    opts.Policies.OnException<ConcurrencyException>().RetryTimes(3);
    opts.Policies
        .OnException<NpgsqlException>()
        .RetryWithCooldown(
        50.Milliseconds(),
        100.Milliseconds(),
        250.Milliseconds());

    opts.Policies.AutoApplyTransactions();
});

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

await app.RunJasperFxCommands(args);
