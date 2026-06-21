using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

namespace UDP_Relay_Service;

/// <summary>
/// Registers the Windows Event Log logging provider. Isolated into a
/// <see cref="SupportedOSPlatformAttribute"/>-annotated method so the platform
/// compatibility analyzer (CA1416) is satisfied when it is called under an
/// <c>OperatingSystem.IsWindows()</c> guard.
/// </summary>
internal static class WindowsEventLogSetup
{
    [SupportedOSPlatform("windows")]
    public static void Add(ILoggingBuilder logging)
    {
        logging.AddEventLog(settings =>
        {
            settings.SourceName = "UDP_Relay_Service";
            settings.LogName = "Application";
        });
    }
}
