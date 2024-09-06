using FluentAssertions;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;
using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Validation;

namespace SFA.DAS.Forecasting.Application.UnitTests.AccountProjection.Queries;

public class WhenGettingProjectionSummaryForAnAccount
{
    private GetAccountProjectionSummaryQueryHandler _handler;
    private Mock<IValidator<GetAccountProjectionSummaryQuery>> _validator;
    private CancellationToken _cancellationToken;
    private GetAccountProjectionSummaryQuery _query;
    private Mock<IAccountProjectionService> _service;
    private const long ExpectedAccountId = 553234;
    private const int NumberOfMonths = 12;

    [SetUp]
    public void Arrange()
    {
        _query = new GetAccountProjectionSummaryQuery { AccountId = ExpectedAccountId, NumberOfMonths = NumberOfMonths };
        _validator = new Mock<IValidator<GetAccountProjectionSummaryQuery>>();
        _validator.Setup(x => x.ValidateAsync(It.IsAny<GetAccountProjectionSummaryQuery>()))
            .ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });
        _cancellationToken = new CancellationToken();
        _service = new Mock<IAccountProjectionService>();
        
        var accountProjectionSummary = new AccountProjectionSummary(
            ExpectedAccountId,
            DateTime.Today,
            12,
            123.56M,
            654.32M);
        
        _service.Setup(x => x.GetProjectionSummary(ExpectedAccountId, DateTime.Today, 12)).ReturnsAsync(accountProjectionSummary);

        _handler = new GetAccountProjectionSummaryQueryHandler(_validator.Object, _service.Object);
    }

    [Test]
    public async Task Then_The_Query_Is_Validated()
    {
        //Act
        await _handler.Handle(_query, _cancellationToken);

        //Assert
        _validator.Verify(x => x.ValidateAsync(_query), Times.Once);

    }

    [Test]
    public void Then_If_The_Query_Fails_Validation_Then_An_Argument_Exception_Is_Thrown()
    {
        //Arrange
        _validator.Setup(x => x.ValidateAsync(It.IsAny<GetAccountProjectionSummaryQuery>()))
            .ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });

        //Act
        var action = () =>  _handler.Handle(_query, _cancellationToken);

        // Assert
        action.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task Then_The_Return_Type_Is_Assigned_To_The_Response()
    {
        //Act
        var actual = await _handler.Handle(_query, _cancellationToken);

        //Assert
        actual.Should().BeAssignableTo<GetAccountProjectionSummaryResult>();
    }

    [Test]
    public async Task Then_The_Repository_Is_Called_With_The_Request_Details()
    {
        //Act
        await _handler.Handle(_query, _cancellationToken);

        //Assert
        _service.Verify(x => x.GetProjectionSummary(ExpectedAccountId, DateTime.Today, NumberOfMonths));
    }

    [Test]
    public async Task Then_The_Values_Are_Returned_In_The_Response()
    {
        //Arrange
        const decimal expectedFundsIn = 100.00M;
        const decimal expectedFundsOut = 999.99M;
        int expectedNumberOfmonths = 6;
        var startDate = DateTime.Today;
        var accountProjectionSummary = new AccountProjectionSummary(ExpectedAccountId, startDate, expectedNumberOfmonths, expectedFundsIn, expectedFundsOut);
        _service.Setup(x => x.GetProjectionSummary(ExpectedAccountId, DateTime.Today, expectedNumberOfmonths)).ReturnsAsync(accountProjectionSummary);
        _query.NumberOfMonths = expectedNumberOfmonths;

        //Act
        var actual = await _handler.Handle(_query, _cancellationToken);

        //Assert
        actual.AccountId.Should().Be(ExpectedAccountId);
        actual.FundsIn.Should().Be(expectedFundsIn);
        actual.FundsOut.Should().Be(expectedFundsOut);
        actual.ProjectionStartDate.Should().Be(DateTime.Today);
        actual.NumberOfMonths.Should().Be(expectedNumberOfmonths);
    }

    [Test]
    public async Task Then_If_The_Service_Returns_Null_Then_Null_Is_Returned()
    {
        //Arrange
        _service.Setup(x => x.GetProjectionSummary(ExpectedAccountId, DateTime.Today, 12)).ReturnsAsync((AccountProjectionSummary)null);

        //Act
        var actual = await _handler.Handle(_query, _cancellationToken);

        //Assert
        actual.Should().BeNull();
    }
}