using System;

namespace SFA.DAS.Forecasting.Domain.AccountProjection
{
    public class AccountProjectionSummary
    {
        public long AccountId { get; }
        public DateTime StartDate { get; }
        public int NumberOfMonths { get; }
        public decimal FundsIn { get; }
        public decimal FundsOut { get; }

        public AccountProjectionSummary(long accountId, DateTime startDate, int numberOfMonths, decimal fundsIn, decimal fundsOut)
        {
            AccountId = accountId;
            StartDate = startDate;
            NumberOfMonths = numberOfMonths;
            FundsIn = fundsIn;
            FundsOut = fundsOut;
        }
    }
}
