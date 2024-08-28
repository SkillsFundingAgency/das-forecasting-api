using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SFA.DAS.Forecasting.Api.Extensions;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;
using SFA.DAS.Forecasting.Application.AccountProjection.Services;
using SFA.DAS.Forecasting.Data.Extensions;
using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Domain.Validation;
using SFA.DAS.Forecasting.Infrastructure.Configuration;

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


        var serviceProvider = services.BuildServiceProvider();
        var azureActiveDirectoryConfiguration = serviceProvider.GetService<IOptions<AzureActiveDirectoryConfiguration>>();
        var forecastingConfiguration = serviceProvider.GetService<IOptions<ForecastingConfiguration>>();

        if (!_configuration["Environment"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
        {
            services.AddAuthorization(o =>
            {
                o.AddPolicy("default", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Default");
                });
            });
            services.AddAuthentication(auth => { auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; }).AddJwtBearer(auth =>
            {
                auth.Authority = $"https://login.microsoftonline.com/{azureActiveDirectoryConfiguration.Value.Tenant}";
                auth.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudiences = azureActiveDirectoryConfiguration.Value.Identifier.Split(",")
                };
            });
            services.AddSingleton<IClaimsTransformation, AzureAdScopeClaimTransformation>();
        }

        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(GetAccountExpiringFundsQueryHandler).Assembly));
        services.AddScoped(typeof(IValidator<GetAccountExpiringFundsQuery>), typeof(GetAccountExpiryValidator));

        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(GetAccountProjectionSummaryQuery).Assembly));
        services.AddScoped(typeof(IValidator<GetAccountProjectionSummaryQuery>),
            typeof(GetAccountProjectionSummaryValidator));
        services.AddScoped(typeof(IValidator<GetAccountProjectionDetailQuery>),
            typeof(GetAccountProjectionDetailValidator));

        services.AddTransient<IAccountProjectionService, AccountProjectionService>();

        services.AddHealthChecks();

        services.AddMvc(o =>
        {
            if (!_configuration["Environment"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                o.Filters.Add(new AuthorizeFilter("default"));
            }
        });

        services.AddForecastingDataContext(forecastingConfiguration.Value.ConnectionString, _configuration["Environment"]);

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