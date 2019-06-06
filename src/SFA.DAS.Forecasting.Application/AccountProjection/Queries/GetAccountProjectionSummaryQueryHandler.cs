using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Validation;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Queries
{
    public class GetAccountProjectionSummaryQueryHandler : IRequestHandler<GetAccountProjectionSummaryQuery, GetAccountProjectionSummaryResult>
    {
        private readonly IValidator<GetAccountProjectionSummaryQuery> _validator;
        private readonly IAccountProjectionService _service;
        const int DefaultNumberOfMonths = 12;

        public GetAccountProjectionSummaryQueryHandler(IValidator<GetAccountProjectionSummaryQuery> validator, IAccountProjectionService service)
        {
            _validator = validator;
            _service = service;
        }

        public async Task<GetAccountProjectionSummaryResult> Handle(GetAccountProjectionSummaryQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ArgumentException("The following parameters have failed validation", validationResult.ValidationDictionary.Select(c => c.Key).Aggregate((item1, item2) => item1 + ", " + item2));
            }

            var projectionSummary = await _service.GetProjectionSummary(request.AccountId, DateTime.Today, DefaultNumberOfMonths);

            if (projectionSummary == null)
            {
                return null;
            }

            return new GetAccountProjectionSummaryResult
            {
                AccountId = projectionSummary.AccountId,
                ProjectionStartDate = DateTime.Today,
                NumberOfMonths = DefaultNumberOfMonths,
                FundsIn = projectionSummary.FundsIn,
                FundsOut = projectionSummary.FundsOut
            };
        }
    }
}