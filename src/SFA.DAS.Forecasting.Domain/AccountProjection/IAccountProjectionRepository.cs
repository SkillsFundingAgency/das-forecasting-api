namespace SFA.DAS.Forecasting.Domain.AccountProjection;

public interface IAccountProjectionRepository
{
    Task<List<Entities.AccountProjection>> GetAccountProjectionByAccountId(long accountId);
}