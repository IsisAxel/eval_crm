using Domain.Entities;
using Infrastructure.DataAccessManager.EFCore.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using static Domain.Common.Constants;

namespace Infrastructure.DataAccessManager.EFCore.Configurations;
public class BudgetAlertRateConfiguration : BaseEntityConfiguration<BudgetAlertRate>
{
    public override void Configure(EntityTypeBuilder<BudgetAlertRate> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Number).HasMaxLength(CodeConsts.MaxLength).IsRequired(false);
        // Propriété Rate
        builder.Property(x => x.Rate)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

        // Propriété AlertDate avec la date actuelle par défaut via SQL Server
        builder.Property(x => x.Date)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

    }
}