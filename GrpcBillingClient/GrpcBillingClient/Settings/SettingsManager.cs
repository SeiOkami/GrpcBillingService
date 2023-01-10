using Microsoft.Extensions.Configuration;

namespace GrpcBillingClient.Settings;

public static class SettingsManager
{
    static public AppSettings Settings { get; }

    static SettingsManager()
    {

        var fileName = "appsettings.json";

        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(fileName).Build();

        var settings = config.GetSection(nameof(AppSettings)).Get<AppSettings>();
        if (settings == null)
            throw new Exception($"File settings not found: {fileName}?");
        else
            Settings = settings;
    }
}
