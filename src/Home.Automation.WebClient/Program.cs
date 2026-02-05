using Home.Automation.WebClient;
using Home.Automation.WebClient.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient<SensorHttpClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration.GetConnectionString("WebApi")!));

builder.Services.AddScoped<DeviceRepository>();
builder.Services.AddScoped<LiveUpdaterService>();

await builder.Build().RunAsync();
