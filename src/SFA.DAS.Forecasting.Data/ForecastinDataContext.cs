using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SFA.DAS.Forecasting.Data.Configuration;
using SFA.DAS.Forecasting.Domain.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;

namespace SFA.DAS.Forecasting.Data
{
    public interface IForecastingDataContext
    {
        DbSet<Domain.Entities.AccountProjection> AccountProjections { get; set; }
        
    }

    public partial class ForecastingDataContext : DbContext, IForecastingDataContext
    {
        private const string AzureResource = "https://database.windows.net/";
        public DbSet<Domain.Entities.AccountProjection> AccountProjections { get; set; }

        private readonly IDbConnection _connection;

        private readonly ForecastingConfiguration _configuration;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        public ForecastingDataContext(DbContextOptions options) : base(options)
        {
        }


        public ForecastingDataContext(IDbConnection connection, DbContextOptions<ForecastingDataContext> options)
            : base(options)
        {
            _connection = connection;
        }

        public ForecastingDataContext(IOptions<ForecastingConfiguration> config, DbContextOptions options, AzureServiceTokenProvider azureServiceTokenProvider) : base(options)
        {
            _configuration = config.Value;
            _azureServiceTokenProvider = azureServiceTokenProvider;
        }       

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();

            if (_configuration == null || _azureServiceTokenProvider == null)
            {
                return;
            }

            var connection = new SqlConnection
            {
                ConnectionString = _configuration.ConnectionString,
                AccessToken = _azureServiceTokenProvider.GetAccessTokenAsync(AzureResource).Result,
            };

            optionsBuilder.UseSqlServer(connection, options =>
                 options.EnableRetryOnFailure(
                     5,
                     TimeSpan.FromSeconds(20),
                     null
                 ));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AccountProjection());

            base.OnModelCreating(modelBuilder);
        }
    }
}
