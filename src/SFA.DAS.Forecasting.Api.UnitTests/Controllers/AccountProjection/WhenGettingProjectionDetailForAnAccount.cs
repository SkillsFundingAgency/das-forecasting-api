using AutoFixture;
using FluentAssertions;
using SFA.DAS.Forecasting.Api.Controllers;
using SFA.DAS.Forecasting.Api.Models;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;

namespace SFA.DAS.Forecasting.Api.UnitTests.Controllers.AccountProjection;

public class WhenGettingProjectionDetailForAnAccount
{
    private AccountProjectionController _accountProjectionController;
    private Mock<IMediator> _mediator;
    private GetAccountProjectionDetailQueryResult _queryResult;

    private readonly Fixture _fixture = new();

    private const long ExpectedAccountId = 123234;
    private const int NumberOfMonths = 12;
    private readonly DateTime _expectedStartDate = DateTime.Today;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();

        _queryResult = _fixture.Create<GetAccountProjectionDetailQueryResult>();

        _mediator.Setup(x => x.Send(It.Is<GetAccountProjectionDetailQuery>(c => c.AccountId.Equals(ExpectedAccountId)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_queryResult);

        _accountProjectionController = new AccountProjectionController(_mediator.Object);
    }

    [Test]
    public async Task Then_The_Projections_Are_Returned()
    {
        //Act
        var actual = await _accountProjectionController.GetProjectedFundingDetail(ExpectedAccountId, _expectedStartDate, NumberOfMonths);

        //Assert
        actual.Should().NotBeNull();
        
        var result = actual as ObjectResult;
        result?.StatusCode.Should().NotBeNull();
        ((HttpStatusCode)result.StatusCode).Should().Be(HttpStatusCode.OK);
        result.Value.Should().NotBeNull();
        
        var actualSummaryResult = result.Value as GetAccountProjectionDetailQueryResult;
        actualSummaryResult.Should().NotBeNull();
        actualSummaryResult.Should().Be(_queryResult);
    }

    [Test]
    public async Task Then_If_Null_Is_Returned_Then_A_Not_Found_Error_Is_Returned()
    {
        //Arrange
        _mediator.Setup(x => x.Send(It.Is<GetAccountProjectionDetailQuery>(c => c.AccountId.Equals(ExpectedAccountId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetAccountProjectionDetailQueryResult)null);

        //Act
        var actual = await _accountProjectionController.GetProjectedFundingDetail(ExpectedAccountId, _expectedStartDate, NumberOfMonths);

        //Assert
        var result = actual as NotFoundResult;
        ((HttpStatusCode)result.StatusCode).Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Then_If_A_Validation_Error_Occurs_A_Bad_Request_Is_Returned_With_Errors()
    {
        //Arrange
        const string expectedValidationMessage = "The following parameters have failed validation";
        const string expectedParam = "AccountId";
        _mediator.Setup(x => x.Send(It.IsAny<GetAccountProjectionDetailQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException(expectedValidationMessage, expectedParam));

        //Act
        var actual = await _accountProjectionController.GetProjectedFundingDetail(0, _expectedStartDate, NumberOfMonths);

        //Assert
        var result = actual as ObjectResult;
        result?.StatusCode.Should().NotBeNull();
        ((HttpStatusCode)result.StatusCode).Should().Be(HttpStatusCode.BadRequest);
        
        var actualError = result.Value as ArgumentErrorViewModel;
        actualError.Should().NotBeNull();
        actualError.Message.Should().Be($"{expectedValidationMessage} (Parameter '{expectedParam}')");
        actualError.Params.Should().Be(expectedParam);
    }
}