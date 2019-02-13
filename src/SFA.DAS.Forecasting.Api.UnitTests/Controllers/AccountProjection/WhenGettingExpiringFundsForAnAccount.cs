using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Forecasting.Api.Controllers;
using SFA.DAS.Forecasting.Api.Models;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;
using SFA.DAS.Forecasting.Domain.AccountProjection;

namespace SFA.DAS.Forecasting.Api.UnitTests.Controllers.AccountProjection
{
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
            _accountExpiryResult = new GetAccountExpiringFundsResult {AccountId = ExpectedAccountId,ExpiryAmounts = new List<ExpiryAmounts>(),ProjectionGenerationDate = DateTime.UtcNow};

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
            Assert.IsNotNull(actual);
            var result = actual as ObjectResult;
            Assert.IsNotNull(result?.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, (HttpStatusCode)result.StatusCode);
            Assert.IsNotNull(result.Value);
            var actualExpiryAmounts = result.Value as GetAccountExpiringFundsResult;
            Assert.IsNotNull(actualExpiryAmounts);
            Assert.AreEqual(_accountExpiryResult.ExpiryAmounts, actualExpiryAmounts.ExpiryAmounts);
        }

        [Test]
        public async Task Then_If_Null_Is_Returned_Then_A_Not_Found_Error_Is_Returned()
        {
            //Arrange
            _mediator.Setup(x => x.Send(It.Is<GetAccountExpiringFundsQuery>(c => c.AccountId.Equals(ExpectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAccountExpiringFundsResult
                {
                    AccountId = null,
                    ExpiryAmounts = null,
                    ProjectionGenerationDate = null
                });

            //Act
            var actual = await _accountProjectionController.GetAccountExpiredFunds(ExpectedAccountId);

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
            _mediator.Setup(x => x.Send(It.IsAny<GetAccountExpiringFundsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException(expectedValidationMessage, expectedParam));

            //Act
            var actual = await _accountProjectionController.GetAccountExpiredFunds(0);

            //Assert
            var result = actual as ObjectResult;
            Assert.IsNotNull(result?.StatusCode);
            Assert.AreEqual(HttpStatusCode.BadRequest, (HttpStatusCode)result.StatusCode);
            var actualError = result.Value as ArgumentErrorViewModel;
            Assert.IsNotNull(actualError);
            Assert.AreEqual($"{expectedValidationMessage}\r\nParameter name: {expectedParam}", actualError.Message);
            Assert.AreEqual(expectedParam, actualError.Params);
        }
    }
}
