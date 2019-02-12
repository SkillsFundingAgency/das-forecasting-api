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

            var projectionDate = projectionForDate.ProjectionCreationDate;

            var expiryAmounts = new List<ExpiryAmounts>();
            foreach (var projection in projections)
            {
                expiryAmounts.Add(
                    new ExpiryAmounts(
                        projection.ExpiredFunds,
                        new DateTime(projection.Year,projection.Month,1)
                        )
                    );
            }
            
            return new AccountProjectionExpiry(expectedAccountId, projectionDate,expiryAmounts);
        }
    }
}
