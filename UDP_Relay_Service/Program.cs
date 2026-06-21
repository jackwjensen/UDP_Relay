using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UDP_Relay_Service;

//*********************************************************************************************************************
// Relay service
// Hosts the UDP relay as a .NET Worker Service. Runs as a Windows Service (via AddWindowsService) or, for testing,
// as a plain console app. Either this or the Relay console must run for the relaying to happen.
//*********************************************************************************************************************

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options => options.ServiceName = "UDP_Relay_Service");
builder.Services.AddHostedService<RelayWorker>();

builder.Logging.ClearProviders();
builder.Logging.AddDebug();
builder.Logging.AddFile(options =>
{
    options.FileName = "UDP_Relay_Service";
    options.Extension = "log";
    // Absolute path so logs land next to the executable even when the service's
    // working directory is C:\Windows\System32.
    options.LogDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
});
if (OperatingSystem.IsWindows())
{
    WindowsEventLogSetup.Add(builder.Logging);
}
builder.Logging.SetMinimumLevel(LogLevel.Debug);

IHost host = builder.Build();
host.Run();
