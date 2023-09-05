using SFA.DAS.Forecasting.Domain.AccountProjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Application.AccountProjection.Services
{
    public class AccountProjectionService : IAccountProjectionService
    {
        private readonly IAccountProjectionRepository _repository;

        public AccountProjectionService(IAccountProjectionRepository repository)
        {
            _repository = repository;
        }

        public async Task<AccountProjectionExpiry> GetExpiringFunds(long expectedAccountId)
        {
            var projections = await _repository.GetAccountProjectionByAccountId(expectedAccountId);

            var projectionForDate = projections.FirstOrDefault();
            if (projectionForDate == null)
            {
                return null;
            }

            var expiryAmounts = projections
                .Where(c => !c.ExpiredFunds.Equals(0))
                .Select(projection =>
                    new ExpiryAmounts(projection.ExpiredFunds, new DateTime(projection.Year, projection.Month, 23)))
                .ToList();

            return new AccountProjectionExpiry(expectedAccountId, projectionForDate.ProjectionCreationDate, expiryAmounts.OrderBy(c => c.PayrollDate).ToList());
        }

        public async Task<AccountProjectionSummary> GetProjectionSummary(long accountId, DateTime startDate, int numberOfMonths = 12)
        {
            var projections = await _repository.GetAccountProjectionByAccountId(accountId);

            var projectionForDate = projections.FirstOrDefault();
            if (projectionForDate == null)
            {
                return null;
            }

            var modifiedProjections = projections.Select(x =>
            {
                var commitedTrainingCost = x.LevyFundedCostOfTraining + x.LevyFundedCompletionPayments;
                var transferOutTotal = x.TransferInCostOfTraining > 0
                ? (x.TransferOutCostOfTraining - x.TransferInCostOfTraining) + (x.TransferOutCompletionPayments - x.TransferInCompletionPayments)
                : x.TransferOutCostOfTraining + x.TransferOutCompletionPayments;
                return new
                {
                    Date = new DateTime(x.Year, x.Month, 1),
                    FundsIn = x.LevyFundsIn,
                    FundsOut = commitedTrainingCost + transferOutTotal
                };
            }).ToList().Where(x => x.Date >= startDate).OrderBy(x => x.Date).Take(numberOfMonths);

            return new AccountProjectionSummary(accountId, startDate, numberOfMonths, modifiedProjections.Sum(x => x.FundsIn), modifiedProjections.Sum(x => x.FundsOut));
        }

        public async Task<AccountProjectionDetail> GetProjectionDetail(long accountId, DateTime startDate, int numberOfMonths)
        {
            var projections = await _repository.GetAccountProjectionByAccountId(accountId);

            var projectionForDate = projections.FirstOrDefault();
            if (projectionForDate == null)
            {
                return new AccountProjectionDetail
                {
                    AccountId = accountId,
                    ProjectionStartDate = startDate,
                    NumberOfMonths = numberOfMonths,
                    Breakdown = new List<AccountProjectionDetail.ProjectionMonth>()
                };
            }

            var modifiedProjections = projections.Where(x => x.Date >= startDate)
                .OrderBy(x => x.Date)
                .Take(numberOfMonths);

            return new AccountProjectionDetail
            {
                AccountId = accountId,
                ProjectionStartDate = startDate,
                NumberOfMonths = numberOfMonths,
                Breakdown = modifiedProjections.Select(x => new AccountProjectionDetail.ProjectionMonth
                {
                    Month = x.Month,
                    Year = x.Year,
                    FundsIn = x.LevyFundsIn,
                    FundsOut = new AccountProjectionDetail.FundsOut
                    {
                        Commitments = x.LevyFundedCostOfTraining + x.LevyFundedCompletionPayments,
                        ApprovedPledgeApplications = x.ApprovedPledgeApplicationCost,
                        AcceptedPledgeApplications = x.AcceptedPledgeApplicationCost,
                        PledgeOriginatedCommitments = x.PledgeOriginatedCommitmentCost,
                        TransferConnections = (x.TransferOutCostOfTraining + x.TransferOutCompletionPayments) - (x.ApprovedPledgeApplicationCost + x.AcceptedPledgeApplicationCost + x.PledgeOriginatedCommitmentCost)
                    }
                }).ToList()
            };
        }
    }
}
