using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Entities;

namespace SFA.DAS.Forecasting.Data.Repository;

public class AccountProjectionRepository : IAccountProjectionRepository
{
    private readonly IForecastingDataContext _forecastingDataContext;

    public AccountProjectionRepository(IForecastingDataContext forecastingDataContext)
    {
        _forecastingDataContext = forecastingDataContext;
    }

    public async Task<List<AccountProjection>> GetAccountProjectionByAccountId(long accountId)
    {
        var projections = await _forecastingDataContext
            .AccountProjections
            .Where(c => c.AccountId.Equals(accountId))
            .ToListAsync();

        return projections;
    }
}