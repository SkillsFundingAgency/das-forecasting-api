using MediatR;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Queries;

public class GetAccountProjectionDetailQuery : IRequest<GetAccountProjectionDetailQueryResult>
{
    public long AccountId { get; set; }
    public int NumberOfMonths { get; set; }
    public DateTime StartDate { get; set; }
}