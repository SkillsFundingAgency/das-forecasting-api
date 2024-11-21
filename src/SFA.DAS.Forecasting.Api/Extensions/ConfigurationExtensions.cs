using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Forecasting.Domain.Configuration;

namespace SFA.DAS.Forecasting.Api.Extensions;

public static class ConfigurationExtensions
{
    public static IConfiguration BuildDasConfiguration(this IConfiguration configuration)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .AddAzureTableStorage(options =>
            {
                options.EnvironmentName = configuration["Environment"];
                options.ConfigurationKeys = configuration["ConfigNames"]!.Split(',');
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.PreFixConfigurationKeys = false;
            });

        return config.Build();
    }

    public static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        
        services.Configure<ForecastingConfiguration>(configuration.GetSection("Forecasting"));
        services.Configure<AzureActiveDirectoryConfiguration>(configuration.GetSection("AzureAd"));

        services.AddSingleton(cfg => cfg.GetService<IOptions<ForecastingConfiguration>>().Value);
        services.AddSingleton(cfg => cfg.GetService<IOptions<AzureActiveDirectoryConfiguration>>().Value);
        
        return services;
    }
}