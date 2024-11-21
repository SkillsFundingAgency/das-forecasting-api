using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Entities;

namespace SFA.DAS.Forecasting.Data.Repository;

public class AccountProjectionRepository(IForecastingDataContext forecastingDataContext) : IAccountProjectionRepository
{
    public async Task<List<AccountProjection>> GetAccountProjectionByAccountId(long accountId)
    {
        return await forecastingDataContext
            .AccountProjections
            .Where(c => c.AccountId.Equals(accountId))
            .ToListAsync();
    }
}