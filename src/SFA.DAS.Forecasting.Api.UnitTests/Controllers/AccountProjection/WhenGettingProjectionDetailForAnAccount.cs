using AutoFixture;
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
    private readonly DateTime ExpectedStartDate = DateTime.Today;

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
        var actual = await _accountProjectionController.GetProjectedFundingDetail(ExpectedAccountId, ExpectedStartDate, NumberOfMonths);

        //Assert
        Assert.That(actual, Is.Not.Null);
        var result = actual as ObjectResult;
        Assert.That(result?.StatusCode, Is.Not.Null);
        Assert.That((HttpStatusCode)result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.Value, Is.Not.Null);
        var actualSummaryResult = result.Value as GetAccountProjectionDetailQueryResult;
        Assert.That(actualSummaryResult, Is.Not.Null);
        Assert.That(actualSummaryResult, Is.EqualTo(_queryResult));
    }

    [Test]
    public async Task Then_If_Null_Is_Returned_Then_A_Not_Found_Error_Is_Returned()
    {
        //Arrange
        _mediator.Setup(x => x.Send(It.Is<GetAccountProjectionDetailQuery>(c => c.AccountId.Equals(ExpectedAccountId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetAccountProjectionDetailQueryResult)null);

        //Act
        var actual = await _accountProjectionController.GetProjectedFundingDetail(ExpectedAccountId, ExpectedStartDate, NumberOfMonths);

        //Assert
        var result = actual as NotFoundResult;
        Assert.That(result?.StatusCode, Is.Not.Null);
        Assert.That((HttpStatusCode)result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
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
        var actual = await _accountProjectionController.GetProjectedFundingDetail(0, ExpectedStartDate, NumberOfMonths);

        //Assert
        var result = actual as ObjectResult;
        Assert.That(result?.StatusCode, Is.Not.Null);
        Assert.That((HttpStatusCode)result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var actualError = result.Value as ArgumentErrorViewModel;
        Assert.That(actualError, Is.Not.Null);
        Assert.That(actualError.Message, Is.EqualTo($"{expectedValidationMessage} (Parameter '{expectedParam}')"));
        Assert.That(actualError.Params, Is.EqualTo(expectedParam));
    }
}