﻿using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;
using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Application.UnitTests.AccountProjection.Queries
{
    public class WhenGettingProjectionDetailForAnAccount
    {
        private GetAccountProjectionDetailQueryHandler _handler;
        private Mock<IValidator<GetAccountProjectionDetailQuery>> _validator;
        private CancellationToken _cancellationToken;
        private GetAccountProjectionDetailQuery _query;
        private Mock<IAccountProjectionService> _service;
        private const long ExpectedAccountId = 553234;
        private const int NumberOfMonths = 12;
        private readonly Fixture _fixture = new Fixture();
        private List<AccountProjectionDetail.ProjectionMonth> _expectedBreakdown;
        private DateTime ExpectedStartDate { get; set; }

        [SetUp]
        public void Arrange()
        {
            ExpectedStartDate = _fixture.Create<DateTime>();

            _query = new GetAccountProjectionDetailQuery { AccountId = ExpectedAccountId, StartDate = ExpectedStartDate, NumberOfMonths = NumberOfMonths };
            _validator = new Mock<IValidator<GetAccountProjectionDetailQuery>>();
            _validator.Setup(x => x.ValidateAsync(It.IsAny<GetAccountProjectionDetailQuery>()))
                .ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });
            _cancellationToken = new CancellationToken();
            _service = new Mock<IAccountProjectionService>();

            _expectedBreakdown = _fixture.CreateMany<AccountProjectionDetail.ProjectionMonth>().ToList();

            var accountProjectionDetail = new AccountProjectionDetail
            {
                AccountId = ExpectedAccountId,
                ProjectionStartDate = ExpectedStartDate,
                NumberOfMonths = NumberOfMonths,
                Breakdown = _expectedBreakdown
            };

            _service.Setup(x => x.GetProjectionDetail(ExpectedAccountId, ExpectedStartDate, 12)).ReturnsAsync(accountProjectionDetail);

            _handler = new GetAccountProjectionDetailQueryHandler(_validator.Object, _service.Object);
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
            _validator.Setup(x => x.ValidateAsync(It.IsAny<GetAccountProjectionDetailQuery>()))
                .ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });

            //Act Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _handler.Handle(_query, _cancellationToken));
        }

        [Test]
        public async Task Then_The_Return_Type_Is_Assigned_To_The_Response()
        {
            //Act
            var actual = await _handler.Handle(_query, _cancellationToken);

            //Assert
            Assert.IsAssignableFrom<GetAccountProjectionDetailQueryResult>(actual);
        }

        [Test]
        public async Task Then_The_Repository_Is_Called_With_The_Request_Details()
        {
            //Act
            await _handler.Handle(_query, _cancellationToken);

            //Assert
            _service.Verify(x => x.GetProjectionDetail(ExpectedAccountId, ExpectedStartDate, NumberOfMonths));
        }

        [Test]
        public async Task Then_The_Values_Are_Returned_In_The_Response()
        {
            //Arrange
            const int expectedNumberOfMonths = 6;

            var accountProjectionDetail = new AccountProjectionDetail
            {
                AccountId = ExpectedAccountId,
                ProjectionStartDate = ExpectedStartDate,
                NumberOfMonths = expectedNumberOfMonths,
                Breakdown = _expectedBreakdown
            };

            _service.Setup(x => x.GetProjectionDetail(ExpectedAccountId, ExpectedStartDate, expectedNumberOfMonths)).ReturnsAsync(accountProjectionDetail);
            _query.NumberOfMonths = expectedNumberOfMonths;

            //Act
            var actual = await _handler.Handle(_query, _cancellationToken);

            //Assert
            Assert.AreEqual(ExpectedAccountId, actual.AccountId);
            Assert.AreEqual(ExpectedStartDate, actual.ProjectionStartDate);
            Assert.AreEqual(expectedNumberOfMonths, actual.NumberOfMonths);
        }

        [Test]
        public async Task Then_If_The_Service_Returns_Null_Then_Null_Is_Returned()
        {
            //Arrange
            _service.Setup(x => x.GetProjectionDetail(ExpectedAccountId, ExpectedStartDate, 12)).ReturnsAsync((AccountProjectionDetail)null);

            //Act
            var actual = await _handler.Handle(_query, _cancellationToken);

            //Assert
            Assert.IsNull(actual);
        }
    }
}
