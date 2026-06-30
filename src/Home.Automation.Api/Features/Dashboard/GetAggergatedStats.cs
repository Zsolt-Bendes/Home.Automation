using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Home.Automation.Api.Features.Dashboard;

public enum Filter
{
    Last24Hours,
    PrevWeek,
    PrevMonth,
}

public sealed record State(DateTimeOffset TimeStamp, double TemperatureInCelsius, double Humidity);

public sealed record PastState(IReadOnlyList<State> States);

public static class GetAggergatedStatsEndpoint
{
    private const string _last24HoursSql = """
        SELECT json_build_object('TimeStamp', "timestamp", 'Humidity', data->'Humidity', 'TemperatureInCelsius', data->'TemperatureInCelsius')
        FROM public.mt_events
        WHERE "timestamp" >= NOW() - INTERVAL '1 day'
        	AND data->>'SensorId' = ?
        	AND type = 'temperature_measurement_received'
        	AND mt_dotnet_type = 'Home.Automation.Api.Domain.TempAndHumiditySensors.Events.TemperatureMeasurementReceived, Home.Automation.Api';
        """;

    private const string _lastWeekSql = """
        SELECT json_build_object('TimeStamp', "timestamp", 'Humidity', data->'Humidity', 'TemperatureInCelsius', data->'TemperatureInCelsius') 
        FROM public.mt_events
        WHERE 
        	data->>'SensorId' = ?
        	AND type = 'temperature_measurement_received'
        	AND mt_dotnet_type = 'Home.Automation.Api.Domain.TempAndHumiditySensors.Events.TemperatureMeasurementReceived, Home.Automation.Api'
        	AND "timestamp" >= date_trunc('week', CURRENT_DATE) - INTERVAL '1 week'
            AND "timestamp" <  date_trunc('week', CURRENT_DATE);
        """;

    private const string _lastMonthSql = """
        SELECT json_build_object('TimeStamp', "timestamp", 'Humidity', data->'Humidity', 'TemperatureInCelsius', data->'TemperatureInCelsius') 
        FROM public.mt_events
        WHERE 
        	data->>'SensorId' = ?
        	AND type = 'temperature_measurement_received'
        	AND mt_dotnet_type = 'Home.Automation.Api.Domain.TempAndHumiditySensors.Events.TemperatureMeasurementReceived, Home.Automation.Api'
        	AND "timestamp" >= date_trunc('month', CURRENT_DATE) - INTERVAL '1 month'
            AND "timestamp" <  date_trunc('month', CURRENT_DATE);
        """;

    internal const string _endpoint = "/dashboard/aggregated-stats";

    [WolverineGet(_endpoint)]
    public static async Task<IResult> Get(
        Filter filter,
        Guid deviceId,
        IQuerySession querySession,
        CancellationToken cancellationToken)
    {
        var view = await LoadDataAsync(filter, deviceId, querySession, cancellationToken);
        if (view is null)
        {
            return Results.Problem(new ProblemDetails
            {
                Title = "Dashboard does not exists yet!",
                Status = 404
            });
        }

        var aggregatedStats = new PastState(view);
        return Results.Ok(aggregatedStats);
    }

    private static Task<IReadOnlyList<State>> LoadDataAsync(
        Filter filter,
        Guid deviceId,
        IQuerySession querySession,
        CancellationToken cancellationToken)
    {
        return filter switch
        {
            Filter.Last24Hours => querySession.QueryAsync<State>(_last24HoursSql, token: cancellationToken, deviceId.ToString()),
            Filter.PrevWeek => querySession.QueryAsync<State>(_lastWeekSql, token: cancellationToken, deviceId.ToString()),
            Filter.PrevMonth => querySession.QueryAsync<State>(_lastMonthSql, token: cancellationToken, deviceId.ToString()),
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };
    }
}
