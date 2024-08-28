using SFA.DAS.Forecasting.Api.Controllers;
using SFA.DAS.Forecasting.Api.Models;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;
using SFA.DAS.Forecasting.Domain.AccountProjection;

namespace SFA.DAS.Forecasting.Api.UnitTests.Controllers.AccountProjection;

public class WhenGettingExpiringFundsForAnAccount
{
    private AccountProjectionController _accountProjectionController;
    private Mock<IMediator> _mediator;
    private GetAccountExpiringFundsResult _accountExpiryResult;
    private const long ExpectedAccountId = 123234;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _accountExpiryResult = new GetAccountExpiringFundsResult { AccountId = ExpectedAccountId, ExpiryAmounts = new List<ExpiryAmounts>(), ProjectionGenerationDate = DateTime.UtcNow };

        _mediator.Setup(x => x.Send(It.Is<GetAccountExpiringFundsQuery>(c => c.AccountId.Equals(ExpectedAccountId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_accountExpiryResult);

        _accountProjectionController = new AccountProjectionController(_mediator.Object);

    }

    [Test]
    public async Task Then_The_Projections_Are_Returned()
    {
        //Act
        var actual = await _accountProjectionController.GetAccountExpiredFunds(ExpectedAccountId);

        //Assert
        Assert.That(actual, Is.Not.Null);
        var result = actual as ObjectResult;
        Assert.That(result?.StatusCode, Is.Not.Null);
        Assert.That((HttpStatusCode)result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.Value, Is.Not.Null);
        var actualExpiryAmounts = result.Value as GetAccountExpiringFundsResult;
        Assert.That(actualExpiryAmounts, Is.Not.Null);
        Assert.That(actualExpiryAmounts.ExpiryAmounts, Is.EqualTo(_accountExpiryResult.ExpiryAmounts));
    }

    [Test]
    public async Task Then_If_Null_Is_Returned_Then_A_Not_Found_Error_Is_Returned()
    {
        //Arrange
        _mediator.Setup(x => x.Send(It.Is<GetAccountExpiringFundsQuery>(c => c.AccountId.Equals(ExpectedAccountId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetAccountExpiringFundsResult)null);

        //Act
        var actual = await _accountProjectionController.GetAccountExpiredFunds(ExpectedAccountId);

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
        _mediator.Setup(x => x.Send(It.IsAny<GetAccountExpiringFundsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException(expectedValidationMessage, expectedParam));

        //Act
        var actual = await _accountProjectionController.GetAccountExpiredFunds(0);

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