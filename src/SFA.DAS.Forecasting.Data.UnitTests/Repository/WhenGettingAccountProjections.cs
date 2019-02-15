using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Forecasting.Data.Repository;
using SFA.DAS.Forecasting.Data.UnitTests.DatabaseMock;
using SFA.DAS.Forecasting.Domain.Entities;

namespace SFA.DAS.Forecasting.Data.UnitTests.Repository
{
    public class WhenGettingAccountProjections
    {
        private Mock<IForecastingDataContext> _forecastingDataContext;
        private AccountProjectionRepository _accountProjectionRepository;

        [SetUp]
        public void Setup()
        {
            var accountProjections = new List<AccountProjection>
            {
                new AccountProjection
                {
                    AccountId = 1,
                    FutureFunds = 100
                },
                new AccountProjection
                {
                    AccountId = 1,
                    FutureFunds = 100
                },
                new AccountProjection
                {
                    AccountId = 2,
                    FutureFunds = 200
                }
            };

            _forecastingDataContext = new Mock<IForecastingDataContext>();
            _forecastingDataContext.Setup(x => x.AccountProjections).ReturnsDbSet(accountProjections);

            _accountProjectionRepository = new AccountProjectionRepository(_forecastingDataContext.Object);
        }

        [Test]
        public async Task Then_The_Results_Are_Filtered_By_AccountId()
        {
            //Arrange
            var accountId = 1;

            //Act
            var actual = await _accountProjectionRepository.GetAccountProjectionByAccountId(accountId);

            //Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Count);
        }

        [Test]
        public async Task Then_An_Empty_List_Is_Returned_When_There_Are_No_Records()
        {
            //Arrange
            var accountId = 3;

            //Act
            var actual = await _accountProjectionRepository.GetAccountProjectionByAccountId(accountId);

            //Assert
            Assert.IsNotNull(actual);
            Assert.IsEmpty(actual);
        }
    }
}
