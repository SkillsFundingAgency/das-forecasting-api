using SFA.DAS.Forecasting.Application.AccountProjection.Services;
using SFA.DAS.Forecasting.Domain.AccountProjection;

namespace SFA.DAS.Forecasting.Application.UnitTests.AccountProjection.Services;

public class WhenGettingProjectionDetailForAnAccount
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
    private const decimal DefaultApprovedPledgeApplicationCost = 5;
    private const decimal DefaultAcceptedPledgeApplicationCost = 6;
    private const decimal DefaultPledgeOriginatedCommitmentCost = 7;

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
                    TransferOutCompletionPayments = DefaultTransferOutCompletionPayments,
                    ApprovedPledgeApplicationCost = DefaultApprovedPledgeApplicationCost,
                    AcceptedPledgeApplicationCost = DefaultAcceptedPledgeApplicationCost,
                    PledgeOriginatedCommitmentCost = DefaultPledgeOriginatedCommitmentCost
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
        var actual = await _accountProjectionService.GetProjectionDetail(ExpectedAccountId, _expectedGenerationDate, 12);

        //Assert
        _accountProjectionRepository.Verify(x => x.GetAccountProjectionByAccountId(ExpectedAccountId));
        Assert.That(actual, Is.Not.Null);
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
        var actual = await _accountProjectionService.GetProjectionDetail(ExpectedAccountId, _expectedGenerationDate, numberOfMonths);

        //Assert
        Assert.That(actual.Breakdown.Sum(x => x.FundsIn), Is.EqualTo(expectedFundsIn));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(6)]
    [TestCase(12)]
    public async Task Then_The_FundsOut_Commitments_Projections_Are_Correct_For_The_Account(int numberOfMonths)
    {
        // arrange
        var expected = (DefaultLevyFundedCostOfTraining + DefaultLevyFundedCompletionPayments) * numberOfMonths;

        //Act
        var actual = await _accountProjectionService.GetProjectionDetail(ExpectedAccountId, _expectedGenerationDate, numberOfMonths);

        //Assert
        Assert.That(actual.Breakdown.Sum(x => x.FundsOut.Commitments), Is.EqualTo(expected));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(6)]
    [TestCase(12)]
    public async Task Then_The_FundsOut_ApprovedPledgeApplications_Projections_Are_Correct_For_The_Account(int numberOfMonths)
    {
        // arrange
        var expected = DefaultApprovedPledgeApplicationCost * numberOfMonths;

        //Act
        var actual = await _accountProjectionService.GetProjectionDetail(ExpectedAccountId, _expectedGenerationDate, numberOfMonths);

        //Assert
        Assert.That(actual.Breakdown.Sum(x => x.FundsOut.ApprovedPledgeApplications), Is.EqualTo(expected));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(6)]
    [TestCase(12)]
    public async Task Then_The_FundsOut_AcceptedPledgeApplications_Projections_Are_Correct_For_The_Account(int numberOfMonths)
    {
        // arrange
        var expected = DefaultAcceptedPledgeApplicationCost * numberOfMonths;

        //Act
        var actual = await _accountProjectionService.GetProjectionDetail(ExpectedAccountId, _expectedGenerationDate, numberOfMonths);

        //Assert
        Assert.That(actual.Breakdown.Sum(x => x.FundsOut.AcceptedPledgeApplications), Is.EqualTo(expected));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(6)]
    [TestCase(12)]
    public async Task Then_The_FundsOut_PledgeOriginatedCommitments_Projections_Are_Correct_For_The_Account(int numberOfMonths)
    {
        // arrange
        var expected = DefaultPledgeOriginatedCommitmentCost * numberOfMonths;

        //Act
        var actual = await _accountProjectionService.GetProjectionDetail(ExpectedAccountId, _expectedGenerationDate, numberOfMonths);

        //Assert
        Assert.That(actual.Breakdown.Sum(x => x.FundsOut.PledgeOriginatedCommitments), Is.EqualTo(expected));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(6)]
    [TestCase(12)]
    public async Task Then_The_FundsOut_TransferConnections_Projections_Are_Correct_For_The_Account(int numberOfMonths)
    {
        // arrange
        var expected = (DefaultTransferOutCompletionPayments + DefaultTransferOutCostOfTraining - DefaultApprovedPledgeApplicationCost - DefaultAcceptedPledgeApplicationCost - DefaultPledgeOriginatedCommitmentCost) * numberOfMonths;

        //Act
        var actual = await _accountProjectionService.GetProjectionDetail(ExpectedAccountId, _expectedGenerationDate, numberOfMonths);

        //Assert
        Assert.That(actual.Breakdown.Sum(x => x.FundsOut.TransferConnections), Is.EqualTo(expected));
    }
}