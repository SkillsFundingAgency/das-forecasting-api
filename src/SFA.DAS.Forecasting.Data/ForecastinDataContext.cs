using System;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Forecasting.Domain.Entities;

namespace SFA.DAS.Forecasting.Data
{
    public interface IForecastingDataContext
    {
        DbSet<Domain.Entities.AccountProjection> AccountProjections { get; set; }
        
        int SaveChanges();
    }

    public partial class ForecastingDataContext : DbContext, IForecastingDataContext
    {
        public DbSet<AccountProjection> AccountProjections { get; set; }
        public ForecastingDataContext()
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        public ForecastingDataContext(DbContextOptions options) : base(options)
        {
        }
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }
    }
}
