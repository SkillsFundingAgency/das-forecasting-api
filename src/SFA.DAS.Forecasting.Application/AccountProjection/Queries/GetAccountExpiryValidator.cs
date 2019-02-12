using System.Threading.Tasks;
using SFA.DAS.Forecasting.Domain.Validation;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Queries
{
    public class GetAccountExpiryValidator : IValidator<GetAccountExpiringFundsQuery>
    {
        public Task<ValidationResult> ValidateAsync(GetAccountExpiringFundsQuery item)
        {
            var validationResult = new ValidationResult();
            if (item.AccountId == 0)
            {
                validationResult.AddError(nameof(item.AccountId));
            }

            return Task.FromResult(validationResult);
        }
    }
}
