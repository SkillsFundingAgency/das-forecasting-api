using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Newtonsoft.Json;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Queries
{
    public class GetAccountProjectionDetailQuery : IRequest<GetAccountProjectionDetailQueryResult>
    {
        public long AccountId { get; set; }
        public int NumberOfMonths { get; set; }
    }

    public class GetAccountProjectionDetailQueryHandler : IRequestHandler<GetAccountProjectionDetailQuery, GetAccountProjectionDetailQueryResult>
    {
        public async Task<GetAccountProjectionDetailQueryResult> Handle(GetAccountProjectionDetailQuery request, CancellationToken cancellationToken)
        {
            return new GetAccountProjectionDetailQueryResult();
        }
    }

    public class GetAccountProjectionDetailQueryResult
    {
        public long AccountId { get; set; }
        public DateTime ProjectionStartDate { get; set; }
        public int NumberOfMonths { get; set; }
        public List<ProjectionMonth> Breakdown { get; set; } = new List<ProjectionMonth>();

        public class ProjectionMonth
        {
            public int Month { get; set; }
            public int Year { get; set; }
            public decimal FundsIn { get; set; }
            public FundsOut FundsOut { get; set; }
        }

        public class FundsOut
        {
            [JsonProperty(PropertyName = "commitments")]
            public decimal Commitments { get; set; }
            [JsonProperty(PropertyName = "approved-pledge-applications")]
            public decimal ApprovedPledgeApplications { get; set; }
            [JsonProperty(PropertyName = "accepted-pledge-applications")]
            public decimal AcceptedPledgeApplications { get; set; }
            [JsonProperty(PropertyName = "pledge-originated-commitments")]
            public decimal PledgeOriginatedCommitments { get; set; }
            [JsonProperty(PropertyName = "transfer-connections")]
            public decimal TransferConnections { get; set; }
        }
    }
}
