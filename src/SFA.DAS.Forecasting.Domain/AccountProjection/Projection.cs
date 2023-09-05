namespace SFA.DAS.Forecasting.Domain.AccountProjection;

public class Projection
{
    public long AccountId { get; }

    public Projection(long accountId)
    {
        AccountId = accountId;
    }
}