# Digital Apprenticeships Service

## das-forecasting-api

Licensed under the [MIT license](https://github.com/SkillsFundingAgency/das-assessor-service/blob/master/LICENSE.txt)

|               |               |
| ------------- | ------------- |
| Solution | SFA.DAS.Forecasting.Api.sln |
| Database | das-[ENV]-fcast-db  |

### Developer Setup

#### Requirements

- Install [.NET Core 6.0 SDK](https://www.microsoft.com/net/download)
- Install [Visual Studio 2022](https://www.visualstudio.com/downloads/) with these workloads:
    - ASP.NET and web development
    - Azure development
- Install [SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- Install [SQL Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- Install [Azure Storage Emulator](https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409) (Make sure you are on atleast v5.3)
- Install [Azure Storage Explorer](http://storageexplorer.com/) 
- Administrator Access

#### Setup

- Clone this repository
- Open Visual Studio as an administrator

##### Config

- Get the das-forecasting-api configuration json file from [das-employer-config](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-provider-registrations/SFA.DAS.ProviderRegistrations.json); which is a non-public repository.
- Create a Configuration table in your (Development) local Azure Storage account.
- Add a row to the Configuration table with fields: 
    - PartitionKey: LOCAL, 
    - RowKey: SFA.DAS.Forecasting.Api_1.0, 
    - Data: {{The contents of the local config json file}}.
- Update Configuration SFA.DAS.Forecasting.Api_1.0, "DatabaseConnectionString":"Data Source={{Local Instance Name}};Database={{Database Name}};Integrated Security = true;Trusted_Connection=True"

##### Open the API project within the solution

- Open Visual studio as an administrator
- Open the solution
- Set SFA.DAS.Forecasting.Api as the startup project
- Running the solution will launch the API in your browser

#### Getting Started

The Forecasting API contains the following endpoints:

- https://localhost:5001/api/accounts/{accountId}/AccountProjection/expiring-funds
- https://localhost:5001/api/accounts/{accountId}/AccountProjection/projected-summary
- https://localhost:5001/api/accounts/{accountId}/AccountProjection/detail
