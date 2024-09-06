using System.IO;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Forecasting.Infrastructure.Configuration;

namespace SFA.DAS.Forecasting.Api.Extensions;

public static class ConfigurationExtensions
{
    public static IConfiguration BuildDasConfiguration(this IConfiguration configuration)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .AddAzureTableStorageConfiguration(
                configuration["ConfigurationStorageConnectionString"],
                configuration["AppName"].Split(","),
                configuration["Environment"],
                configuration["Version"]
            );

        return config.Build();
    }
}