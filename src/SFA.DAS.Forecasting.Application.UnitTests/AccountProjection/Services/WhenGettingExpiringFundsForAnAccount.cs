﻿using SFA.DAS.Forecasting.Application.AccountProjection.Services;
using SFA.DAS.Forecasting.Domain.AccountProjection;

namespace SFA.DAS.Forecasting.Application.UnitTests.AccountProjection.Services;

public class WhenGettingExpiringFundsForAnAccount
{
    private Mock<IAccountProjectionRepository> _accountProjectionRepository;
    private AccountProjectionService _accountProjectionService;
    private List<Domain.Entities.AccountProjection> _expectedProjection;
    private const long ExpectedAccountId = 55437;
    private readonly DateTime _expectedGenerationDate = new(2018, 10, 24);

    [SetUp]
    public void Arrange()
    {
        _expectedProjection = new List<Domain.Entities.AccountProjection>
        {
            new()
            {
                AccountId = ExpectedAccountId,
                ExpiredFunds = 150.55m,
                Month = 10,
                Year = 2018,
                ProjectionCreationDate = _expectedGenerationDate
            },
            new()
            {
                AccountId = ExpectedAccountId,
                ExpiredFunds = 0m,
                Month = 7,
                Year = 2018,
                ProjectionCreationDate = _expectedGenerationDate
            },
            new()
            {
                AccountId = ExpectedAccountId,
                ExpiredFunds = 170.55m,
                Month = 8,
                Year = 2018,
                ProjectionCreationDate = _expectedGenerationDate
            }
        };

        _accountProjectionRepository = new Mock<IAccountProjectionRepository>();
        _accountProjectionRepository
            .Setup(x => x.GetAccountProjectionByAccountId(ExpectedAccountId))
            .ReturnsAsync(_expectedProjection);

        _accountProjectionService = new AccountProjectionService(_accountProjectionRepository.Object);
    }

    [Test]
    public async Task Then_The_Projections_Are_Taken_From_The_Repository_For_The_Account()
    {
        //Act
        var actual = await _accountProjectionService.GetExpiringFunds(ExpectedAccountId);

        //Assert
        _accountProjectionRepository.Verify(x => x.GetAccountProjectionByAccountId(ExpectedAccountId));
        Assert.IsNotNull(actual);
    }

    [Test]
    public async Task Then_If_There_Are_No_Projections_Null_Is_Returned()
    {
        //Arrange
        _accountProjectionRepository
            .Setup(x => x.GetAccountProjectionByAccountId(11))
            .ReturnsAsync(new List<Domain.Entities.AccountProjection>());

        //Act
        var actual = await _accountProjectionService.GetExpiringFunds(11);

        //Assert
        Assert.IsNull(actual);
    }

    [Test]
    public async Task Then_The_Results_Are_Mapped_To_The_Domain_Model()
    {
        //Act
        var actual = await _accountProjectionService.GetExpiringFunds(ExpectedAccountId);

        //Assert
        Assert.AreEqual(ExpectedAccountId, actual.AccountId);
        Assert.AreEqual(_expectedGenerationDate, actual.ProjectionGenerationDate);
        var expiryAmounts = actual.ExpiryAmounts.LastOrDefault();
        Assert.IsNotNull(expiryAmounts);
        Assert.AreEqual(_expectedProjection[0].ExpiredFunds, expiryAmounts.Amount);
        Assert.AreEqual(new DateTime(_expectedProjection[0].Year, _expectedProjection[0].Month, 23), expiryAmounts.PayrollDate);
    }

    [Test]
    public async Task Then_Zero_Values_Are_Filtered_Out_And_The_Results_Are_Ordered_Date_Asc()
    {
        //Act
        var actual = await _accountProjectionService.GetExpiringFunds(ExpectedAccountId);

        //Assert
        Assert.AreEqual(2, actual.ExpiryAmounts.Count);
        Assert.AreEqual(_expectedProjection[2].ExpiredFunds, actual.ExpiryAmounts[0].Amount);
    }
}