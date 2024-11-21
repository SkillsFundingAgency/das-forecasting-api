using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;
using SFA.DAS.Forecasting.Domain.Validation;

namespace SFA.DAS.Forecasting.Api.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<GetAccountExpiringFundsQuery>, GetAccountExpiryValidator>();
        services.AddScoped<IValidator<GetAccountExpiringFundsQuery>, GetAccountExpiryValidator>();
        services.AddScoped<IValidator<GetAccountProjectionSummaryQuery>, GetAccountProjectionSummaryValidator>();
        services.AddScoped<IValidator<GetAccountProjectionDetailQuery>, GetAccountProjectionDetailValidator>();
        
        return services;
    }
}