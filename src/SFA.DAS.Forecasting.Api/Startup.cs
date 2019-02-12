using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Forecasting.Domain.Configuration;
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
                )
                .Build();
            Configuration = config;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<AzureActiveDirectoryConfiguration>(Configuration.GetSection("AzureAd"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<AzureActiveDirectoryConfiguration>>().Value);

            var serviceProvider = services.BuildServiceProvider();
            var azureActiveDirectoryConfiguration = serviceProvider.GetService<IOptions<AzureActiveDirectoryConfiguration>>();

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

            services.AddMvc(o =>
            {
                if (!Configuration["Environment"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
                {
                    o.Filters.Add(new AuthorizeFilter("default"));
                }


            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseAuthentication();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
