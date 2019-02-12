using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Domain.AccountProjection
{
    public interface IAccountProjectionService
    {
        Task<AccountProjectionExpiry> GetExpiringFunds(long expectedAccountId);
    }
}