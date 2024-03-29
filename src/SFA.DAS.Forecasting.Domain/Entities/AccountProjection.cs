﻿namespace SFA.DAS.Forecasting.Domain.Entities;

public class AccountProjection
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public DateTime ProjectionCreationDate { get; set; }
    public byte ProjectionGenerationType { get; set; }
    public short Month { get; set; }
    public int Year { get; set; }
    public decimal LevyFundsIn { get; set; }
    public decimal LevyFundedCostOfTraining { get; set; }
    public decimal LevyFundedCompletionPayments { get; set; }
    public decimal TransferInCostOfTraining { get; set; }
    public decimal TransferOutCostOfTraining { get; set; }
    public decimal TransferInCompletionPayments { get; set; }
    public decimal TransferOutCompletionPayments { get; set; }
    public decimal ExpiredFunds { get; set; }
    public decimal FutureFunds { get; set; }
    public decimal CoInvestmentEmployer { get; set; }
    public decimal CoInvestmentGovernment { get; set; }
    public decimal ApprovedPledgeApplicationCost { get; set; }
    public decimal AcceptedPledgeApplicationCost { get; set; }
    public decimal PledgeOriginatedCommitmentCost { get; set; }
    public DateTime Date => new(Year, Month, 1);
}