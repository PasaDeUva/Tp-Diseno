using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TP_DisenoDB.Domain.Entities;

namespace TP_DisenoDB.Infrastructure.Relational.Configurations;

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasDiscriminator<string>("PurchaseType")
               .HasValue<CashPurchase>("Cash")
               .HasValue<MonthlyPayments>("Monthly");

        builder.HasMany(p => p.AppliedPromotions)
               .WithMany(pr => pr.AppliedToPurchases);
    }
}
