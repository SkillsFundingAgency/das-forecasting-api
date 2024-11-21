using MediatR;
using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Validation;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Queries;

public class GetAccountProjectionSummaryQueryHandler(IValidator<GetAccountProjectionSummaryQuery> validator, IAccountProjectionService service)
    : IRequestHandler<GetAccountProjectionSummaryQuery, GetAccountProjectionSummaryResult>
{
    public async Task<GetAccountProjectionSummaryResult> Handle(GetAccountProjectionSummaryQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid())
        {
            throw new ArgumentException("The following parameters have failed validation", validationResult.ValidationDictionary.Select(c => c.Key).Aggregate((item1, item2) => item1 + ", " + item2));
        }

        var projectionSummary = await service.GetProjectionSummary(request.AccountId, DateTime.Today, request.NumberOfMonths);

        if (projectionSummary == null)
        {
            return null;
        }

        return new GetAccountProjectionSummaryResult
        {
            AccountId = projectionSummary.AccountId,
            ProjectionStartDate = DateTime.Today,
            NumberOfMonths = request.NumberOfMonths,
            FundsIn = projectionSummary.FundsIn,
            FundsOut = projectionSummary.FundsOut
        };
    }
}