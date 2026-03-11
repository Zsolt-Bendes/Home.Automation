using Alba;
using Marten;

namespace Home.Automation.IntegrationTests;

public abstract class AlbaTestBase(AlbaBootstrap albaBootstrap)
{
    protected IAlbaHost Host => albaBootstrap.Host;

    protected IDocumentStore Store => albaBootstrap.Store;
}