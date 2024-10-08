﻿using FluentAssertions;
using SFA.DAS.Forecasting.Application.AccountProjection.Queries;

namespace SFA.DAS.Forecasting.Application.UnitTests.AccountProjection.Queries;

public class WhenValidatingTheRequestToGetExpiringFundsForAnAccount
{
    private GetAccountExpiryValidator _validator;

    [SetUp]
    public void Arrange()
    {
        _validator = new GetAccountExpiryValidator();
    }

    [Test]
    public async Task Then_The_Query_Is_Invalid_If_The_Required_Fields_Are_Not_Passed_And_Validation_Errors_Returned()
    {
        //Act
        var actual = await _validator.ValidateAsync(new GetAccountExpiringFundsQuery());

        //Assert
        actual.IsValid().Should().BeFalse();
        actual.ValidationDictionary.ContainsValue("AccountId has not been supplied").Should().BeTrue();
    }

    [Test]
    public async Task Then_The_Query_Is_Valid_If_The_Values_Are_Valid()
    {
        //Act
        var actual = await _validator.ValidateAsync(new GetAccountExpiringFundsQuery { AccountId = 99432 });

        //Assert
        actual.IsValid().Should().BeTrue();
    }
}