namespace Home.Automation.Api.Infrastructure.Settings;

public sealed class EmailSettings
{
    public string ApiUrl { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string Sender { get; set; } = string.Empty;
}