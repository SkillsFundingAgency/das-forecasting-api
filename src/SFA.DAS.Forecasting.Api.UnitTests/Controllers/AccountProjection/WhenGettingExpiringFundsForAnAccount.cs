using FluentAssertions;
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

        _mediator.Setup(expression: x => x.Send(It.Is<GetAccountExpiringFundsQuery>(c => c.AccountId.Equals(ExpectedAccountId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: _accountExpiryResult);

        _accountProjectionController = new AccountProjectionController(mediator: _mediator.Object);
    }

    [Test]
    public async Task Then_The_Projections_Are_Returned()
    {
        //Act
        var actual = await _accountProjectionController.GetAccountExpiredFunds(accountId: ExpectedAccountId);

        //Assert
        actual.Should().NotBeNull();

        var result = actual as ObjectResult;
        result?.StatusCode.Should().NotBeNull();
        ((HttpStatusCode)result.StatusCode).Should().Be(expected: (HttpStatusCode.OK));
        result.Value.Should().NotBeNull();

        var actualExpiryAmounts = result.Value as GetAccountExpiringFundsResult;
        actualExpiryAmounts.Should().NotBeNull();
        actualExpiryAmounts.ExpiryAmounts.Should().BeEquivalentTo(expectation: _accountExpiryResult.ExpiryAmounts);
    }

    [Test]
    public async Task Then_If_Null_Is_Returned_Then_A_Not_Found_Error_Is_Returned()
    {
        //Arrange
        _mediator.Setup(expression: x => x.Send(It.Is<GetAccountExpiringFundsQuery>(c => c.AccountId.Equals(ExpectedAccountId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: null);

        //Act
        var actual = await _accountProjectionController.GetAccountExpiredFunds(accountId: ExpectedAccountId);

        //Assert
        var result = actual as NotFoundResult;
        ((HttpStatusCode)result.StatusCode).Should().Be(expected: HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Then_If_A_Validation_Error_Occurs_A_Bad_Request_Is_Returned_With_Errors()
    {
        //Arrange
        const string expectedValidationMessage = "The following parameters have failed validation";
        const string expectedParam = "AccountId";
        _mediator.Setup(expression: x => x.Send(It.IsAny<GetAccountExpiringFundsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception: new ArgumentException(message: expectedValidationMessage, paramName: expectedParam));

        //Act
        var actual = await _accountProjectionController.GetAccountExpiredFunds(accountId: 0);

        //Assert
        var result = actual as ObjectResult;
        ((HttpStatusCode)result.StatusCode).Should().Be(expected: HttpStatusCode.BadRequest);

        var actualError = result.Value as ArgumentErrorViewModel;
        actualError.Should().NotBeNull();
        actualError.Message.Should().Be(expected: $"{expectedValidationMessage} (Parameter '{expectedParam}')");
        actualError.Params.Should().Be(expectedParam);
    }
}