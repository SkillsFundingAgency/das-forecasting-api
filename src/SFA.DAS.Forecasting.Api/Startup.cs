using System;
using System.Collections.Generic;
using System.IO;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;
using SFA.DAS.Forecasting.Application.AccountProjection.Services;
using SFA.DAS.Forecasting.Data;
using SFA.DAS.Forecasting.Data.Repository;
using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Domain.Validation;
using SFA.DAS.Forecasting.Infrastructure.Configuration;

namespace SFA.DAS.Forecasting.Api
{
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
            services.Configure<ForecastingConfiguration>(Configuration.GetSection("Forecasting"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<ForecastingConfiguration>>().Value);
            services.Configure<AzureActiveDirectoryConfiguration>(Configuration.GetSection("AzureAd"));
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
                        ValidAudiences = new List<string>
                        {
                            azureActiveDirectoryConfiguration.Value.Identifier,
                            azureActiveDirectoryConfiguration.Value.Id
                        }
                    };
                });
                services.AddSingleton<IClaimsTransformation, AzureAdScopeClaimTransformation>();
            }

            services.AddMediatR(typeof(GetAccountExpiringFundsQueryHandler).Assembly);
            services.AddScoped(typeof(IValidator<GetAccountExpiringFundsQuery>), typeof(GetAccountExpiryValidator));

            services.AddMediatR(typeof(GetAccountProjectionSummaryQuery).Assembly);
            services.AddScoped(typeof(IValidator<GetAccountProjectionSummaryQuery>),
                typeof(GetAccountProjectionSummaryValidator));

            services.AddTransient<IAccountProjectionRepository, AccountProjectionRepository>();
            services.AddTransient<IAccountProjectionService, AccountProjectionService>();

            services.AddHealthChecks();

            services.AddMvc(o =>
            {
                if (!Configuration["Environment"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
                {
                    o.Filters.Add(new AuthorizeFilter("default"));
                }


            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            
            services.AddDbContext<ForecastingDataContext>(options => options.UseSqlServer(forecastingConfiguration.Value.ConnectionString));
            services.AddScoped<IForecastingDataContext, ForecastingDataContext>(provider => provider.GetService<ForecastingDataContext>());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseHealthChecks("/health");
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
