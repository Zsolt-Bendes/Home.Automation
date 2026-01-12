using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Home.Automation.Api.Features.Dashboard;

public static class GetDashboardEndpoint
{
    internal const string _endpoint = "/dashboard";

    [WolverineGet(_endpoint)]
    public static async Task<IResult> Get(IQuerySession querySession, CancellationToken cancellationToken)
    {
        var view = await querySession.LoadAsync<DashboardView>(1, cancellationToken);
        if (view is null)
        {
            return Results.Problem(new ProblemDetails
            {
                Title = "Dashboard does not exists yet!",
                Status = 404
            });
        }

        return Results.Ok(view);
    }
}
