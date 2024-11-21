﻿using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Forecasting.Data.Repository;
using SFA.DAS.Forecasting.Domain.AccountProjection;

namespace SFA.DAS.Forecasting.Data.Extensions;

public static class ServiceCollectionExtensions
{
    private const string AzureResource = "https://database.windows.net/";
    private static readonly AzureServiceTokenProvider TokenProvider = new();
    
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