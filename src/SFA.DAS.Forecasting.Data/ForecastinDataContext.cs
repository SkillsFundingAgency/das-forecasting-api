﻿using SFA.DAS.Forecasting.Data.Configuration;


namespace SFA.DAS.Forecasting.Data;

public interface IForecastingDataContext
{
    DbSet<Domain.Entities.AccountProjection> AccountProjections { get; set; }

}

public class ForecastingDataContext(DbContextOptions options) : DbContext(options), IForecastingDataContext
{
    public DbSet<Domain.Entities.AccountProjection> AccountProjections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccountProjection());

        base.OnModelCreating(modelBuilder);
    }
}