using System;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Queries;

public class GetAccountProjectionSummaryResult
{
    public long AccountId { get; set; }
    public DateTime ProjectionStartDate { get; set; }
    public int NumberOfMonths { get; set; }
    public decimal FundsIn { get; set; }
    public decimal FundsOut { get; set; }
}