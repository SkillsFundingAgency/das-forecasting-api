using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Validation;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Queries
{
    public class GetAccountExpiringFundsQueryHandler : IRequestHandler<GetAccountExpiringFundsQuery, GetAccountExpiringFundsResult>
    {
        private readonly IValidator<GetAccountExpiringFundsQuery> _validator;
        private readonly IAccountProjectionService _service;

        public GetAccountExpiringFundsQueryHandler(IValidator<GetAccountExpiringFundsQuery> validator, IAccountProjectionService service)
        {
            _validator = validator;
            _service = service;
        }

        public async Task<GetAccountExpiringFundsResult> Handle(GetAccountExpiringFundsQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ArgumentException("The following parameters have failed validation", validationResult.ValidationDictionary.Select(c => c.Key).Aggregate((item1, item2) => item1 + ", " + item2));
            }

            var expiringFunds = await _service.GetExpiringFunds(request.AccountId);

            return new GetAccountExpiringFundsResult
            {
                AccountId = expiringFunds.AccountId,
                ExpiryAmounts = expiringFunds.ExpiryAmounts,
                ProjectionGenerationDate = expiringFunds.ProjectionGenerationDate
            };
        }
    }
}