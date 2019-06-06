using MediatR;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Queries
{
    public class GetAccountProjectionSummaryQuery : IRequest<GetAccountProjectionSummaryResult>
    {
        public long AccountId { get; set; }
        public int NumberOfMonths { get; set; }
    }
}
