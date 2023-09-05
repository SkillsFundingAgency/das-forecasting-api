using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Forecasting.Data.Repository;
using SFA.DAS.Forecasting.Domain.AccountProjection;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddForecastingDataContext(this IServiceCollection services, string connectionString, string environmentName)
    {
        services.AddDbContext<IForecastingDataContext, ForecastingDataContext>((serviceProvider, options) =>
        {
            var connection = new SqlConnection(connectionString);

            if (!environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                var generateTokenTask = GenerateTokenAsync();
                connection.AccessToken = generateTokenTask.GetAwaiter().GetResult();
            }

            options
                .UseLazyLoadingProxies()
                .UseSqlServer(
                    connection,
                    o => o.CommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds));
        });
        RegisterServices(services);
        return services;
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IAccountProjectionRepository, AccountProjectionRepository>();

    }

    public static async Task<string> GenerateTokenAsync()
    {
        const string AzureResource = "https://database.windows.net/";
        var azureServiceTokenProvider = new AzureServiceTokenProvider();
        var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(AzureResource);

        return accessToken;
    }
}