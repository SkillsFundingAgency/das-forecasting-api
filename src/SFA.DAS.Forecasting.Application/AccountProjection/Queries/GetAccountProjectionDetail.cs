using SFA.DAS.Forecasting.Domain.Validation;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Queries;

public class GetAccountProjectionDetailValidator : IValidator<GetAccountProjectionDetailQuery>
{
    public Task<ValidationResult> ValidateAsync(GetAccountProjectionDetailQuery item)
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