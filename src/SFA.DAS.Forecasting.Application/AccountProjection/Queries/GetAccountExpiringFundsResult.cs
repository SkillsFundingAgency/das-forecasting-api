using SFA.DAS.Forecasting.Domain.AccountProjection;
using System;
using System.Collections.Generic;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Queries;

public class GetAccountExpiringFundsResult
{
    public long? AccountId { get; set; }
    public DateTime? ProjectionGenerationDate { get; set; }
    public List<ExpiryAmounts> ExpiryAmounts { get; set; }
}