using Home.Automation.Api.Infrastructure.Exceptions;
using Home.Automation.Api.Infrastructure.Settings;
using Home.Automation.Api.Services.Email;
using Home.Automation.Api.Services.Email.Client;
using JasperFx.Events.Daemon;
using Marten;
using System.Net.Http.Headers;
using System.Text;
using Wolverine.Http;
using Wolverine.Marten;

namespace Home.Automation.Api.Infrastructure;

public static class ConfigurationExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMarten(configuration)
            .AddWolverineHttp()
            .AddEmailRelatedDependencies(configuration)
            .AddTimeProvider();
    }

    private static IServiceCollection AddMarten(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("postgres");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new PostgresConnectionStringInsNullOrEmptyException();
        }

        services.AddMarten(new MartenConfigurations(connectionString))
            .AddAsyncDaemon(DaemonMode.Solo)
            .IntegrateWithWolverine(ops => ops.UseFastEventForwarding = true)
            .UseLightweightSessions();

        return services;
    }

    private static IServiceCollection AddTimeProvider(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        return services;
    }

    private static IServiceCollection AddEmailRelatedDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("MailSettings"));

        services.AddHttpClient<IEmailHttpClient, EmailHttpClient>(config =>
        {
            var url = configuration.GetValue<string>("MailSettings:ApiUrl");
            var key = configuration.GetValue<string>("MailSettings:ApiKey");

            ArgumentException.ThrowIfNullOrEmpty(url);
            ArgumentException.ThrowIfNullOrEmpty(key);

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{key}"));

            config.BaseAddress = new Uri(url);
            config.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        });

        services.AddTransient<IEmailService, EmailService>();
        return services;
    }
}