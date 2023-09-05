using Microsoft.EntityFrameworkCore;
using SFA.DAS.Forecasting.Data.Configuration;


namespace SFA.DAS.Forecasting.Data;

public interface IForecastingDataContext
{
    DbSet<Domain.Entities.AccountProjection> AccountProjections { get; set; }

}

public class ForecastingDataContext : DbContext, IForecastingDataContext
{

    public DbSet<Domain.Entities.AccountProjection> AccountProjections { get; set; }

    public ForecastingDataContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccountProjection());

        base.OnModelCreating(modelBuilder);
    }
}