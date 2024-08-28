using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Forecasting.Domain.Configuration;
using SFA.DAS.Forecasting.Infrastructure.Configuration;

namespace SFA.DAS.Forecasting.Infrastructure.UnitTests.Configuration;

public class WhenParsingConfigurationFromStorage
{
    private StorageConfigParser _storageConfigParser;

    [SetUp]
    public void Arrange()
    {
        _storageConfigParser = new StorageConfigParser();
    }

    [Test]
    public void Then_The_Data_Is_Not_Added_To_The_Dictionary_If_Not_Valid()
    {
        //Arrange
        var configItem = new ConfigurationItem { Data = "{" };

        //Act 
        var action = () => _storageConfigParser.ParseConfig(configItem, "");
        
        //Assert
        action.Should().Throw<JsonReaderException>();
    }

    [Test]
    public void Then_The_Data_Is_Not_Added_To_The_Dictionary_It_Does_Not_Exist()
    {
        //Arrange
        var configItem = new ConfigurationItem { Data = "{}" };

        //Act
        var actual = _storageConfigParser.ParseConfig(configItem, "");

        //Assert
        actual.Should().NotBeNull();
    }

    [Test]
    public void Then_The_Properties_Are_Correctly_Added_To_The_Dictionary_For_Simple_Data_Structures()
    {
        //Arrange
        var configItem = new ConfigurationItem { Data = "{\"Configuration\":{\"Item1\":\"Value1\"}}" };

        //Act
        var actual = _storageConfigParser.ParseConfig(configItem, "");

        //Assert
        actual.Should().NotBeNull();
        actual.Should().NotBeEmpty();
        actual.Should().ContainKey("Configuration:Item1").WhoseValue.Should().Be("Value1");
    }

    [Test]
    public void Then_The_Default_SectionName_Is_Used_For_Single_Level_ConfigurationObjects()
    {
        //Arrange
        var configItem = new ConfigurationItem { Data = "{\"Item1\":\"Value1\",\"Item2\":\"Value2\"}" };

        //Act
        var actual = _storageConfigParser.ParseConfig(configItem, "Section");

        //Assert
        actual.Should().NotBeNull();
        actual.Should().NotBeEmpty();
        actual.Should().ContainKey("Section:Item1").WhoseValue.Should().Be("Value1");
        actual.Should().ContainKey("Section:Item2").WhoseValue.Should().Be("Value2");
    }

    [Test]
    public void Then_Complex_Configuration_Structures_Are_Added_To_The_Dictionary()
    {
        //Arrange
        var configItem = new ConfigurationItem { Data = "{\"Item1\":\"Value1\",\"Item2\":\"Value2\",\"Configuration\":{\"Item1\":\"Value1\",\"Item2\":\"Value2\"}, \"Configuration2\":{\"Item3\":\"Value3\"}}" };

        //Act
        var actual = _storageConfigParser.ParseConfig(configItem, "Section");

        //Assert
        actual.Should().NotBeNull();
        actual.Should().NotBeEmpty();
        actual.Count.Should().Be(5);
        
        actual.Should().ContainKey("Configuration:Item1").WhoseValue.Should().Be("Value1");
        actual.Should().ContainKey("Configuration:Item2").WhoseValue.Should().Be("Value2");
        actual.Should().ContainKey("Configuration2:Item3").WhoseValue.Should().Be("Value3");

        actual.Should().ContainKey("Section:Item1").WhoseValue.Should().Be("Value1");
        actual.Should().ContainKey("Section:Item2").WhoseValue.Should().Be("Value2");
    }
}