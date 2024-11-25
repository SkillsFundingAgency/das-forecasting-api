using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Infrastructure.Configuration;

namespace SFA.DAS.Forecasting.Api.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddApiAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
        {
            return services;
        }
        
        services
            .AddAuthorizationBuilder()
            .AddPolicy("default", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Default");
            });

        var azureActiveDirectoryConfiguration = configuration
            .GetSection("AzureAd")
            .Get<AzureActiveDirectoryConfiguration>();

        services.AddAuthentication(auth => { auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; }).AddJwtBearer(auth =>
        {
            auth.Authority = $"https://login.microsoftonline.com/{azureActiveDirectoryConfiguration.Tenant}";
            auth.TokenValidationParameters = new TokenValidationParameters
            {
                ValidAudiences = azureActiveDirectoryConfiguration.Identifier.Split(",")
            };
        });

        services.AddSingleton<IClaimsTransformation, AzureAdScopeClaimTransformation>();

        return services;
    }
}