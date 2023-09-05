using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Domain.AccountProjection;

public interface IAccountProjectionRepository
{
    Task<List<Entities.AccountProjection>> GetAccountProjectionByAccountId(long accountId);
}