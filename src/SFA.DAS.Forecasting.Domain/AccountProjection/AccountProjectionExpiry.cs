using System;
using System.Collections.Generic;

namespace SFA.DAS.Forecasting.Domain.AccountProjection
{
    public class AccountProjectionExpiry
    {
        public long AccountId { get; }
        public DateTime ProjectionGenerationDate { get; }
        public List<ExpiryAmounts> ExpiryAmounts { get; }

        public AccountProjectionExpiry(long accountId, DateTime projectionGenerationDate, List<ExpiryAmounts> expiryAmounts)
        {
            AccountId = accountId;
            ProjectionGenerationDate = projectionGenerationDate;
            ExpiryAmounts = expiryAmounts;
        }
    }
}