using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SFA.DAS.Forecasting.Domain.Configuration;

namespace SFA.DAS.Forecasting.Infrastructure.Configuration;

public class AzureTableStorageConfigurationProvider : ConfigurationProvider
{
    private readonly string _connection;
    private readonly string _environment;
    private readonly string _version;
    private readonly string[] _appName;


    public AzureTableStorageConfigurationProvider(string connection, string[] appName, string environment, string version)
    {
        _connection = connection;
        _environment = environment;
        _version = version;
        _appName = appName;
    }

    public override void Load()
    {
        var table = GetTable();
        foreach (var config in _appName)
        {
            var configParams = config.Split(":");
            var configDefaultSectionName = configParams.Length > 1 ? configParams[1] : string.Empty;

            var operation = GetOperation(configParams[0], _environment, _version);
            var result = table.ExecuteAsync(operation).Result;

            var configItem = (ConfigurationItem)result.Result;

            var data = new StorageConfigParser().ParseConfig(configItem, configDefaultSectionName);
            data.ToList().ForEach(x => Data.Add(x.Key, x.Value));
        }
    }



    private CloudTable GetTable()
    {
        var storageAccount = CloudStorageAccount.Parse(_connection);
        var tableClient = storageAccount.CreateCloudTableClient();
        return tableClient.GetTableReference("Configuration");
    }

    private TableOperation GetOperation(string serviceName, string environmentName, string version)
    {
        return TableOperation.Retrieve<ConfigurationItem>(environmentName, $"{serviceName}_{version}");
    }
}