using Microsoft.EntityFrameworkCore;
using TP_DisenoDB.Domain.Entities;
using TP_DisenoDB.Infrastructure.Relational.Configurations;

namespace TP_DisenoDB.Infrastructure.Relational.DbContext;

public class MySqlDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public MySqlDbContext(DbContextOptions<MySqlDbContext> options) : base(options) { }

    public DbSet<Bank> Banks { get; set; }
    public DbSet<CardHolder> CardHolders { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<CashPurchase> CashPurchases { get; set; }
    public DbSet<MonthlyPayments> MonthlyPayments { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<DiscountPromotion> DiscountPromotions { get; set; }
    public DbSet<FinancingPromotion> FinancingPromotions { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Quota> Quotas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MySqlDbContext).Assembly);
        
        // Relationship between Payment and Quota / CashPurchase
        modelBuilder.Entity<Payment>()
            .HasMany(p => p.Quotas)
            .WithOne(q => q.Payment)
            .HasForeignKey(q => q.PaymentId);

        modelBuilder.Entity<Payment>()
            .HasMany(p => p.CashPurchases)
            .WithOne(cp => cp.Payment)
            .HasForeignKey(cp => cp.PaymentId);
    }
}
