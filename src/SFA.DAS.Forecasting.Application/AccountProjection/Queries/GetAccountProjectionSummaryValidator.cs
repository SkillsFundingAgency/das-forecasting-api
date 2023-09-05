using SFA.DAS.Forecasting.Domain.Validation;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Queries;

public class GetAccountProjectionSummaryValidator : IValidator<GetAccountProjectionSummaryQuery>
{
    public Task<ValidationResult> ValidateAsync(GetAccountProjectionSummaryQuery item)
    {
        var validationResult = new ValidationResult();
        if (item.AccountId == 0)
        {
            validationResult.AddError(nameof(item.AccountId));
        }

        switch (item.NumberOfMonths)
        {
            case < 0:
            case > 24:
                validationResult.AddError(nameof(item.NumberOfMonths));
                break;
        }

        return Task.FromResult(validationResult);
    }
}