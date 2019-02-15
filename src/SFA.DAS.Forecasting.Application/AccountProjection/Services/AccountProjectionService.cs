﻿using System;
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
    }
}
