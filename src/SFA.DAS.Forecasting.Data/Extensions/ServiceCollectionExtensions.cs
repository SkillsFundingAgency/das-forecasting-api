using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Forecasting.Data.Repository;
using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Configuration;

namespace SFA.DAS.Forecasting.Data.Extensions;

public static class ServiceCollectionExtensions
{
    private const string AzureResource = "https://database.windows.net/";
    private static readonly AzureServiceTokenProvider TokenProvider = new();

    public static IServiceCollection AddForecastingDataContext(this IServiceCollection services, string environmentName)
    {
        services.AddDbContext<IForecastingDataContext, ForecastingDataContext>((serviceProvider, options) =>
        {
            var forecastingConfiguration = serviceProvider.GetService<ForecastingConfiguration>();

            if (forecastingConfiguration == null || string.IsNullOrEmpty(forecastingConfiguration.ConnectionString))
            {
                var logger = serviceProvider.GetService<ILogger>();
                var config = serviceProvider.GetService<IConfiguration>();
                logger.LogError("The connection string is not configured correctly. Config values: {Values}", JsonConvert.SerializeObject(config.GetChildren()));
            }

            var connection = new SqlConnection(forecastingConfiguration.ConnectionString);

            if (!environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                var generateTokenTask = GenerateTokenAsync();
                connection.AccessToken = generateTokenTask.GetAwaiter().GetResult();
            }

            options
                .UseLazyLoadingProxies()
                .UseSqlServer(connection, o => o.CommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds));
        });

        services.AddTransient<IAccountProjectionRepository, AccountProjectionRepository>();

        return services;
    }

    private static async Task<string> GenerateTokenAsync()
    {
        return await TokenProvider.GetAccessTokenAsync(AzureResource);
    }
}