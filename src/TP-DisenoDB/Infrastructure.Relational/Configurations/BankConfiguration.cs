using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TP_DisenoDB.Domain.Entities;

namespace TP_DisenoDB.Infrastructure.Relational.Configurations;

public class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.HasKey(b => b.Id);
        builder.HasMany(b => b.Promotions)
               .WithOne(p => p.Bank)
               .HasForeignKey(p => p.BankId);
    }
}
