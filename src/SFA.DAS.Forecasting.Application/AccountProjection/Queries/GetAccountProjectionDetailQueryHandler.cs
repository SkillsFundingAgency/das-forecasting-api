using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Validation;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Queries
{
    public class GetAccountProjectionDetailQueryHandler : IRequestHandler<GetAccountProjectionDetailQuery, GetAccountProjectionDetailQueryResult>
    {
        private readonly IValidator<GetAccountProjectionDetailQuery> _validator;
        private readonly IAccountProjectionService _service;

        public GetAccountProjectionDetailQueryHandler(IValidator<GetAccountProjectionDetailQuery> validator, IAccountProjectionService service)
        {
            _validator = validator;
            _service = service;
        }

        public async Task<GetAccountProjectionDetailQueryResult> Handle(GetAccountProjectionDetailQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ArgumentException("The following parameters have failed validation", validationResult.ValidationDictionary.Select(c => c.Key).Aggregate((item1, item2) => item1 + ", " + item2));
            }

            var projectionSummary = await _service.GetProjectionDetail(request.AccountId, request.StartDate, request.NumberOfMonths);

            if (projectionSummary == null)
            {
                return null;
            }

            return new GetAccountProjectionDetailQueryResult
            {
                AccountId = request.AccountId,
                ProjectionStartDate = request.StartDate,
                NumberOfMonths = request.NumberOfMonths,
                Breakdown = projectionSummary.Breakdown.Select(x => new GetAccountProjectionDetailQueryResult.ProjectionMonth
                {
                    Month = x.Month,
                    Year = x.Year,
                    FundsIn = x.FundsIn,
                    FundsOut = new GetAccountProjectionDetailQueryResult.FundsOut
                    {
                        AcceptedPledgeApplications = x.FundsOut.AcceptedPledgeApplications,
                        ApprovedPledgeApplications = x.FundsOut.ApprovedPledgeApplications,
                        Commitments = x.FundsOut.Commitments,
                        PledgeOriginatedCommitments = x.FundsOut.PledgeOriginatedCommitments,
                        TransferConnections = x.FundsOut.TransferConnections
                    }
                }).ToList()
                
            };
        }
    }
}