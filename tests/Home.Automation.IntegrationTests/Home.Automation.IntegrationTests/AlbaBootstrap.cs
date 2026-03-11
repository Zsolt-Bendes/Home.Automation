using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using TUnit.Core.Interfaces;

namespace Home.Automation.IntegrationTests;

public sealed class AlbaBootstrap : IAsyncInitializer, IAsyncDisposable
{
    private RabbitMqContainer _rabbitMqContainer = null!;
    private PostgreSqlContainer PostgreSqlContainer = null!;

    public IDocumentStore Store { get; private set; } = null!;
    public IAlbaHost Host { get; private set; } = null!;


    public async Task InitializeAsync()
    {
        Host = await AlbaHost.For<Program>();

        Store = Host.Services.GetRequiredService<IDocumentStore>();
    }

    public async ValueTask DisposeAsync()
    {
        await Host.DisposeAsync();
    }
}
