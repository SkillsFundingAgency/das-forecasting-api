using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using SFA.DAS.Forecasting.Api.Extensions;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;
using SFA.DAS.Forecasting.Application.AccountProjection.Services;
using SFA.DAS.Forecasting.Data.Extensions;
using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Configuration;

namespace SFA.DAS.Forecasting.Api;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration.BuildDasConfiguration();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions();

        services.AddLogging(builder =>
        {
            builder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
            builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
        });

        services.Configure<ForecastingConfiguration>(_configuration.GetSection("Forecasting"));
        services.Configure<AzureActiveDirectoryConfiguration>(_configuration.GetSection("AzureAd"));

        services.AddSingleton(cfg => cfg.GetService<IOptions<ForecastingConfiguration>>().Value);
        services.AddSingleton(cfg => cfg.GetService<IOptions<AzureActiveDirectoryConfiguration>>().Value);

        services.AddApiAuthorization(_configuration);

        services.AddValidators();

        services.AddTransient<IAccountProjectionService, AccountProjectionService>();

        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(GetAccountExpiringFundsQueryHandler).Assembly));

        services.AddHealthChecks();

        services.AddMvc(o =>
        {
            if (!_configuration["Environment"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                o.Filters.Add(new AuthorizeFilter("default"));
            }
        });

        var forecastingConfiguration = _configuration
            .GetSection("Forecasting")
            .Get<ForecastingConfiguration>();

        services.AddForecastingDataContext(forecastingConfiguration.ConnectionString, _configuration["Environment"]);

        services.AddApplicationInsightsTelemetry();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
            app.UseAuthentication();
        }

        app.UseRouting();

        app.UseHealthChecks("/health");
        app.UseHttpsRedirection();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}