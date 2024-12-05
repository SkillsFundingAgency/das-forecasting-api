using MediatR;
using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Validation;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Queries;

public class GetAccountExpiringFundsQueryHandler(IValidator<GetAccountExpiringFundsQuery> validator, IAccountProjectionService service)
    : IRequestHandler<GetAccountExpiringFundsQuery, GetAccountExpiringFundsResult>
{
    public async Task<GetAccountExpiringFundsResult> Handle(GetAccountExpiringFundsQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid())
        {
            throw new ArgumentException("The following parameters have failed validation", validationResult.ValidationDictionary.Select(c => c.Key).Aggregate((item1, item2) => item1 + ", " + item2));
        }

        var expiringFunds = await service.GetExpiringFunds(request.AccountId);

        if (expiringFunds == null)
        {
            return null;
        }

        return new GetAccountExpiringFundsResult
        {
            AccountId = expiringFunds.AccountId,
            ExpiryAmounts = expiringFunds.ExpiryAmounts,
            ProjectionGenerationDate = expiringFunds.ProjectionGenerationDate
        };
    }
}