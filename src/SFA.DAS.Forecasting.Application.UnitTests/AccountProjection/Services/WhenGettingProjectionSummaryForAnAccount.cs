using SFA.DAS.Forecasting.Application.AccountProjection.Services;
using SFA.DAS.Forecasting.Domain.AccountProjection;

namespace SFA.DAS.Forecasting.Application.UnitTests.AccountProjection.Services;

public class WhenGettingProjectionSummaryForAnAccount
{
    private Mock<IAccountProjectionRepository> _accountProjectionRepository;
    private AccountProjectionService _accountProjectionService;
    private List<Domain.Entities.AccountProjection> _expectedProjection;
    private const long ExpectedAccountId = 55437;
    private readonly DateTime _expectedGenerationDate = new(2018, 10, 24);
    private const decimal DefaultLevyFundsIn = 1;
    private const decimal DefaultTransferInCostOfTraining = 2;
    private const decimal DefaultTransferInCompletionPayments = 3;
    private const decimal DefaultLevyFundedCostOfTraining = 1;
    private const decimal DefaultTransferOutCostOfTraining = 2;
    private const decimal DefaultLevyFundedCompletionPayments = 3;
    private const decimal DefaultTransferOutCompletionPayments = 4;

    [SetUp]
    public void Arrange()
    {
        _expectedProjection = new List<Domain.Entities.AccountProjection>();

        for (int i = -1; i < 14; i++)
        {
            var startDate = _expectedGenerationDate.AddMonths(i);

            _expectedProjection.Add(
                new Domain.Entities.AccountProjection
                {
                    AccountId = ExpectedAccountId,
                    ExpiredFunds = 150.55m,
                    Month = (short)startDate.Month,
                    Year = startDate.Year,
                    ProjectionCreationDate = _expectedGenerationDate,
                    LevyFundsIn = DefaultLevyFundsIn,
                    TransferInCostOfTraining = DefaultTransferInCostOfTraining,
                    TransferInCompletionPayments = DefaultTransferInCompletionPayments,
                    LevyFundedCostOfTraining = DefaultLevyFundedCostOfTraining,
                    TransferOutCostOfTraining = DefaultTransferOutCostOfTraining,
                    LevyFundedCompletionPayments = DefaultLevyFundedCompletionPayments,
                    TransferOutCompletionPayments = DefaultTransferOutCompletionPayments
                }
            );
        }

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
        var actual = await _accountProjectionService.GetProjectionSummary(ExpectedAccountId, _expectedGenerationDate);

        //Assert
        _accountProjectionRepository.Verify(x => x.GetAccountProjectionByAccountId(ExpectedAccountId));
        Assert.IsNotNull(actual);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(6)]
    [TestCase(12)]
    public async Task Then_The_FundsIn_Projections_Are_Correct_For_The_Account(int numberOfMonths)
    {
        // arrange
        var expectedFundsIn = (DefaultLevyFundsIn) * numberOfMonths;

        //Act
        var actual = await _accountProjectionService.GetProjectionSummary(ExpectedAccountId, _expectedGenerationDate, numberOfMonths);

        //Assert
        Assert.AreEqual(expectedFundsIn, actual.FundsIn);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(6)]
    [TestCase(12)]
    public async Task Then_The_FundsOut_Projections_Are_Correct_For_The_Account(int numberOfMonths)
    {
        // arrange
        var expectedFundsOut = (DefaultLevyFundedCostOfTraining + DefaultLevyFundedCompletionPayments + (DefaultTransferOutCostOfTraining + DefaultTransferOutCompletionPayments) - (DefaultTransferInCostOfTraining + DefaultTransferInCompletionPayments)) * numberOfMonths;

        //Act
        var actual = await _accountProjectionService.GetProjectionSummary(ExpectedAccountId, _expectedGenerationDate, numberOfMonths);

        //Assert
        Assert.AreEqual(expectedFundsOut, actual.FundsOut);
    }

    [Test]
    public async Task Then_If_There_Are_No_Projections_Null_Is_Returned()
    {
        //Arrange
        _accountProjectionRepository
            .Setup(x => x.GetAccountProjectionByAccountId(11))
            .ReturnsAsync(new List<Domain.Entities.AccountProjection>());

        //Act
        var actual = await _accountProjectionService.GetProjectionSummary(11, _expectedGenerationDate);

        //Assert
        Assert.IsNull(actual);
    }
}