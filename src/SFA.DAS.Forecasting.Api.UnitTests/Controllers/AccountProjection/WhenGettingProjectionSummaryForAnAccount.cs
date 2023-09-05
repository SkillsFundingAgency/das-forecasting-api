using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Forecasting.Api.Controllers;
using SFA.DAS.Forecasting.Api.Models;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

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
        Assert.IsNotNull(actual);
        var result = actual as ObjectResult;
        Assert.IsNotNull(result?.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, (HttpStatusCode)result.StatusCode);
        Assert.IsNotNull(result.Value);
        var actualSummaryResult = result.Value as GetAccountProjectionSummaryResult;
        Assert.IsNotNull(actualSummaryResult);
        Assert.AreEqual(_accountSummaryResult.FundsIn, actualSummaryResult.FundsIn);
        Assert.AreEqual(_accountSummaryResult.FundsOut, actualSummaryResult.FundsOut);
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
        Assert.IsNotNull(result?.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (HttpStatusCode)result.StatusCode);
    }

    [Test]
    public async Task Then_If_A_Validation_Error_Occurs_A_Bad_Request_Is_Returned_With_Errors()
    {
        //Arrange
        var expectedValidationMessage = "The following parameters have failed validation";
        var expectedParam = "AccountId";
        _mediator.Setup(x => x.Send(It.IsAny<GetAccountProjectionSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException(expectedValidationMessage, expectedParam));

        //Act
        var actual = await _accountProjectionController.GetProjectedFundingSummary(0, NumberOfMonths);

        //Assert
        var result = actual as ObjectResult;
        Assert.IsNotNull(result?.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (HttpStatusCode)result.StatusCode);
        var actualError = result.Value as ArgumentErrorViewModel;
        Assert.IsNotNull(actualError);
        Assert.AreEqual($"{expectedValidationMessage} (Parameter '{expectedParam}')", actualError.Message);
        Assert.AreEqual(expectedParam, actualError.Params);
    }
}