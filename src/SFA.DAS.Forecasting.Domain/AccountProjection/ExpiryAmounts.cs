using System;

namespace SFA.DAS.Forecasting.Domain.AccountProjection;

public class ExpiryAmounts
{
    public decimal Amount { get; }
    public DateTime PayrollDate { get; }

    public ExpiryAmounts(decimal amount, DateTime payrollDate)
    {
        Amount = amount;
        PayrollDate = payrollDate;
    }
}