using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TP_DisenoDB.Domain.Entities;

namespace TP_DisenoDB.Infrastructure.Relational.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasDiscriminator<string>("PromotionType")
               .HasValue<DiscountPromotion>("Discount")
               .HasValue<FinancingPromotion>("Financing");
    }
}
