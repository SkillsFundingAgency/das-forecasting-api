using SFA.DAS.Forecasting.Application.AccountProjection.Queries;

namespace SFA.DAS.Forecasting.Application.UnitTests.AccountProjection.Queries;

public class WhenValidatingTheRequestToGetProjectionSummaryForAnAccount
{
    private GetAccountProjectionSummaryValidator _validator;

    [SetUp]
    public void Arrange()
    {
        _validator = new GetAccountProjectionSummaryValidator();
    }

    [Test]
    public async Task Then_The_Query_Is_Invalid_If_The_Required_Fields_Are_Not_Passed_And_Validation_Errors_Returned()
    {
        //Act
        var actual = await _validator.ValidateAsync(new GetAccountProjectionSummaryQuery());

        //Assert
        Assert.IsFalse(actual.IsValid());
        Assert.IsTrue(actual.ValidationDictionary.ContainsValue("AccountId has not been supplied"));
    }

    [Test]
    public async Task Then_The_Query_Is_Valid_If_The_Values_Are_Valid()
    {
        //Act
        var actual = await _validator.ValidateAsync(new GetAccountProjectionSummaryQuery { AccountId = 99432 });

        //Assert
        Assert.IsTrue(actual.IsValid());
    }
}