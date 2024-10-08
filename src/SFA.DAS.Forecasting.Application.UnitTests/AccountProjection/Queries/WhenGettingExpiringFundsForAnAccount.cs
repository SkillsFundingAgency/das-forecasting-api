﻿using FluentAssertions;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;
using SFA.DAS.Forecasting.Domain.AccountProjection;
using SFA.DAS.Forecasting.Domain.Validation;

namespace SFA.DAS.Forecasting.Application.UnitTests.AccountProjection.Queries;

public class WhenGettingExpiringFundsForAnAccount
{
    private GetAccountExpiringFundsQueryHandler _handler;
    private Mock<IValidator<GetAccountExpiringFundsQuery>> _validator;
    private CancellationToken _cancellationToken;
    private GetAccountExpiringFundsQuery _query;
    private Mock<IAccountProjectionService> _service;
    private const long ExpectedAccountId = 553234;

    [SetUp]
    public void Arrange()
    {
        _query = new GetAccountExpiringFundsQuery { AccountId = ExpectedAccountId };
        _validator = new Mock<IValidator<GetAccountExpiringFundsQuery>>();
        _validator.Setup(x => x.ValidateAsync(It.IsAny<GetAccountExpiringFundsQuery>()))
            .ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });
        _cancellationToken = new CancellationToken();
        _service = new Mock<IAccountProjectionService>();
        var accountProjectionExpiry = new AccountProjectionExpiry(
            ExpectedAccountId,
            new DateTime(2018, 01, 01),
            new List<ExpiryAmounts>
            {
                new(1, new DateTime(2019,01,01))
            });
        _service.Setup(x => x.GetExpiringFunds(ExpectedAccountId)).ReturnsAsync(accountProjectionExpiry);

        _handler = new GetAccountExpiringFundsQueryHandler(_validator.Object, _service.Object);
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
        _validator.Setup(x => x.ValidateAsync(It.IsAny<GetAccountExpiringFundsQuery>()))
            .ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });

        //Act Assert
        var action = () =>  _handler.Handle(_query, _cancellationToken);
        action.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task Then_The_Return_Type_Is_Assigned_To_The_Response()
    {
        //Act
        var actual = await _handler.Handle(_query, _cancellationToken);

        //Assert
        actual.Should().BeAssignableTo<GetAccountExpiringFundsResult>();
    }

    [Test]
    public async Task Then_The_Repository_Is_Called_With_The_Request_Details()
    {
        //Act
        await _handler.Handle(_query, _cancellationToken);

        //Assert
        _service.Verify(x => x.GetExpiringFunds(ExpectedAccountId));
    }

    [Test]
    public async Task Then_The_Values_Are_Returned_In_The_Response()
    {
        //Arrange
        const int expectedAmount = 100;
        var expectedDate = new DateTime(2019, 02, 20);
        var accountProjectionExpiry = new AccountProjectionExpiry(ExpectedAccountId, expectedDate, new List<ExpiryAmounts> { new(expectedAmount, expectedDate) });
        _service.Setup(x => x.GetExpiringFunds(ExpectedAccountId)).ReturnsAsync(accountProjectionExpiry);

        //Act
        var actual = await _handler.Handle(_query, _cancellationToken);

        //Assert
        actual.ExpiryAmounts.Should().NotBeNull();
        actual.ExpiryAmounts[0].Amount.Should().Be(expectedAmount);
        actual.AccountId.Should().Be(ExpectedAccountId);
        actual.ProjectionGenerationDate.Should().Be(expectedDate);
    }

    [Test]
    public async Task Then_If_The_Service_Returns_Null_Then_Null_Is_Returned()
    {
        //Arrange
        _service.Setup(x => x.GetExpiringFunds(ExpectedAccountId)).ReturnsAsync((AccountProjectionExpiry)null);

        //Act
        var actual = await _handler.Handle(_query, _cancellationToken);

        //Assert
        actual.Should().BeNull();
    }
}