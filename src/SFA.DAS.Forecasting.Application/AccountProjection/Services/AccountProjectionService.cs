using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Forecasting.Domain.AccountProjection;

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

            return new AccountProjectionExpiry(expectedAccountId, projectionForDate.ProjectionCreationDate, expiryAmounts.OrderBy(c=>c.PayrollDate).ToList());
        }

        /*
        public async Task<dynamic> GetFinanceProjections(long accountId)
        {
            var projections = await _repository.GetAccountProjectionByAccountId(accountId);

            var projectionForDate = projections.FirstOrDefault();
            if (projectionForDate == null)
            {
                return null;
            }

            var ModifiedProjections = projections.Select(x => new dynamic
            {
                Date = new DateTime(x.Year, x.Month, 1),
                FundsInTotal = x.LevyFundsIn + x.TransferInCostOfTraining + x.TransferInCompletionPayments,
                FundsOutTotal = x.LevyFundedCostOfTraining + x.TransferOutCostOfTraining +
                                x.LevyFundedCompletionPayments + x.TransferOutCompletionPayments
            }).ToList().Where(x => x.Date >= DateTime.Today).OrderBy(x => x.Date).Take(12);

            return ModifiedProjections;
        }
        */

        public async Task<AccountProjectionSummary> GetProjectionSummary(long accountId, DateTime startDate, int numberOfMonths = 12)
        {
            var projections = await _repository.GetAccountProjectionByAccountId(accountId);

            var projectionForDate = projections.FirstOrDefault();
            if (projectionForDate == null)
            {
                return null;
            }

            IEnumerable<dynamic> modifiedProjections = projections.Select(x => new
            {
                Date = new DateTime(x.Year, x.Month, 1),
                FundsInTotal = x.LevyFundsIn + x.TransferInCostOfTraining + x.TransferInCompletionPayments,
                FundsOutTotal = x.LevyFundedCostOfTraining + x.TransferOutCostOfTraining +
                                x.LevyFundedCompletionPayments + x.TransferOutCompletionPayments
            }).ToList().Where(x => x.Date >= startDate).OrderBy(x => x.Date).Take(numberOfMonths);

            return new AccountProjectionSummary(accountId, startDate, numberOfMonths, modifiedProjections.Sum(x => x.FundsIn), modifiedProjections.Sum(x => x.FundsOut));
        }
    }
}
