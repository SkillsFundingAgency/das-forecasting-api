using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;
using SFA.DAS.Forecasting.Application.AccountProjection.Services;
using SFA.DAS.Forecasting.Data.Extensions;
using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Domain.Validation;
using SFA.DAS.Forecasting.Infrastructure.Configuration;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace SFA.DAS.Forecasting.Api;

public class Startup
{
    public Startup(IConfiguration configuration)
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
            ).Build();
        Configuration = config;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions();
        
        services.AddLogging(builder =>
        {
            builder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
            builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
        });
        services.Configure<ForecastingConfiguration>(Configuration.GetSection("Forecasting"));
        services.Configure<AzureActiveDirectoryConfiguration>(Configuration.GetSection("AzureAd"));

        services.AddSingleton(cfg => cfg.GetService<IOptions<ForecastingConfiguration>>().Value);
        services.AddSingleton(cfg => cfg.GetService<IOptions<AzureActiveDirectoryConfiguration>>().Value);


        var serviceProvider = services.BuildServiceProvider();
        var azureActiveDirectoryConfiguration = serviceProvider.GetService<IOptions<AzureActiveDirectoryConfiguration>>();
        var forecastingConfiguration = serviceProvider.GetService<IOptions<ForecastingConfiguration>>();

        if (!Configuration["Environment"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
        {
            services.AddAuthorization(o =>
            {
                o.AddPolicy("default", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Default");
                });
            });
            services.AddAuthentication(auth =>
            {
                auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(auth =>
            {
                auth.Authority = $"https://login.microsoftonline.com/{azureActiveDirectoryConfiguration.Value.Tenant}";
                auth.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
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
            if (!Configuration["Environment"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                o.Filters.Add(new AuthorizeFilter("default"));
            }


        });

        services.AddForecastingDataContext(forecastingConfiguration.Value.ConnectionString.ToString(), Configuration["Environment"]);

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