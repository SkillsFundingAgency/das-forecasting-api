﻿using FluentAssertions;
using SFA.DAS.Forecasting.Api.Controllers;
using SFA.DAS.Forecasting.Api.Models;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;

namespace SFA.DAS.Forecasting.Api.UnitTests.Controllers.AccountProjection;

public class WhenGettingProjectionSummaryForAnAccount
{
    private AccountProjectionController _accountProjectionController;
    private Mock<IMediator> _mediator;
    private GetAccountProjectionSummaryResult _accountSummaryResult;
    private const long ExpectedAccountId = 123234;
    private const decimal ExpectedFundsIn = 1110;
    private const decimal ExpectedFundsOut = 9990;
    private const int NumberOfMonths = 12;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _accountSummaryResult = new GetAccountProjectionSummaryResult
        {
            AccountId = ExpectedAccountId,
            FundsIn = ExpectedFundsIn,
            FundsOut = ExpectedFundsOut,
            NumberOfMonths = 12,
            ProjectionStartDate = DateTime.UtcNow
        };

        _mediator.Setup(x => x.Send(It.Is<GetAccountProjectionSummaryQuery>(c => c.AccountId.Equals(ExpectedAccountId)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_accountSummaryResult);

        _accountProjectionController = new AccountProjectionController(_mediator.Object);
    }

    [Test]
    public async Task Then_The_Projections_Are_Returned()
    {
        //Act
        var actual = await _accountProjectionController.GetProjectedFundingSummary(ExpectedAccountId, NumberOfMonths);

        //Assert
        actual.Should().NotBeNull();

        var result = actual as ObjectResult;
        result?.StatusCode.Should().NotBeNull();
        ((HttpStatusCode)result.StatusCode).Should().Be(HttpStatusCode.OK);
        result.Value.Should().NotBeNull();

        var actualSummaryResult = result.Value as GetAccountProjectionSummaryResult;
        actualSummaryResult.Should().NotBeNull();
        actualSummaryResult.FundsIn.Should().Be(_accountSummaryResult.FundsIn);
        actualSummaryResult.FundsOut.Should().Be(_accountSummaryResult.FundsOut);
    }

    [Test]
    public async Task Then_If_Null_Is_Returned_Then_A_Not_Found_Error_Is_Returned()
    {
        //Arrange
        _mediator.Setup(x => x.Send(It.Is<GetAccountProjectionSummaryQuery>(c => c.AccountId.Equals(ExpectedAccountId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetAccountProjectionSummaryResult)null);

        //Act
        var actual = await _accountProjectionController.GetProjectedFundingSummary(ExpectedAccountId, NumberOfMonths);

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
        _mediator.Setup(x => x.Send(It.IsAny<GetAccountProjectionSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException(expectedValidationMessage, expectedParam));

        //Act
        var actual = await _accountProjectionController.GetProjectedFundingSummary(0, NumberOfMonths);

        //Assert
        var result = actual as ObjectResult;
        ((HttpStatusCode)result.StatusCode).Should().Be(HttpStatusCode.BadRequest);

        var actualError = result.Value as ArgumentErrorViewModel;
        actualError.Should().NotBeNull();
        actualError.Message.Should().Be($"{expectedValidationMessage} (Parameter '{expectedParam}')");
        actualError.Params.Should().Be(expectedParam);
    }
}