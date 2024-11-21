using MediatR;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Queries;

public class GetAccountExpiringFundsQuery : IRequest<GetAccountExpiringFundsResult>
{
    public long AccountId { get; set; }
}