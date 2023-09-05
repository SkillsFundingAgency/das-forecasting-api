using System;
using System.Collections.Generic;

namespace SFA.DAS.Forecasting.Domain.AccountProjection;

public class AccountProjectionDetail
{
    public long AccountId { get; set; }
    public DateTime ProjectionStartDate { get; set; }
    public int NumberOfMonths { get; set; }
    public List<ProjectionMonth> Breakdown { get; set; } = new();

    public class ProjectionMonth
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal FundsIn { get; set; }
        public FundsOut FundsOut { get; set; }
    }

    public class FundsOut
    {
        public decimal Commitments { get; set; }
        public decimal ApprovedPledgeApplications { get; set; }
        public decimal AcceptedPledgeApplications { get; set; }
        public decimal PledgeOriginatedCommitments { get; set; }
        public decimal TransferConnections { get; set; }
    }
}