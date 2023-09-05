using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SFA.DAS.Forecasting.Data.Configuration;

public class AccountProjection : IEntityTypeConfiguration<Domain.Entities.AccountProjection>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.AccountProjection> builder)
    {
        builder.ToTable("AccountProjection");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName(@"Id").HasColumnType("bigint").IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.AccountId).HasColumnName(@"EmployerAccountId").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.ProjectionCreationDate).HasColumnName(@"ProjectionCreationDate").HasColumnType("datetime").IsRequired();
        builder.Property(x => x.ProjectionGenerationType).HasColumnName(@"ProjectionGenerationType").HasColumnType("byte").IsRequired();
        builder.Property(x => x.Month).HasColumnName(@"Month").HasColumnType("smallint").IsRequired();
        builder.Property(x => x.Year).HasColumnName(@"Year").HasColumnType("int").IsRequired();

        builder.Property(x => x.LevyFundsIn).HasColumnName(@"FundsIn").HasColumnType("decimal(18,5)").IsRequired();
        builder.Property(x => x.LevyFundedCostOfTraining).HasColumnName(@"TotalCostOfTraining").HasColumnType("decimal(18,5)").IsRequired();
        builder.Property(x => x.LevyFundedCompletionPayments).HasColumnName(@"CompletionPayments").HasColumnType("decimal(18,5)").IsRequired();

        builder.Property(x => x.TransferInCostOfTraining).HasColumnName(@"TransferInTotalCostOfTraining").HasColumnType("decimal(18,5)").IsRequired();
        builder.Property(x => x.TransferOutCostOfTraining).HasColumnName(@"TransferOutTotalCostOfTraining").HasColumnType("decimal(18,5)").IsRequired();

        builder.Property(x => x.TransferInCompletionPayments).HasColumnName(@"TransferInCompletionPayments").HasColumnType("decimal(18,5)").IsRequired();
        builder.Property(x => x.TransferOutCompletionPayments).HasColumnName(@"TransferOutCompletionPayments").HasColumnType("decimal(18,5)").IsRequired();

        builder.Property(x => x.FutureFunds).HasColumnName(@"FutureFunds").HasColumnType("decimal(18,5)").IsRequired();
        builder.Property(x => x.CoInvestmentEmployer).HasColumnName(@"CoInvestmentEmployer").HasColumnType("decimal(18,5)").IsRequired();
        builder.Property(x => x.CoInvestmentGovernment).HasColumnName(@"CoInvestmentGovernment").HasColumnType("decimal(18,5)").IsRequired();
        builder.Property(x => x.ExpiredFunds).HasColumnName(@"ExpiredFunds").HasColumnType("decimal(18,5)").IsRequired();
    }
}