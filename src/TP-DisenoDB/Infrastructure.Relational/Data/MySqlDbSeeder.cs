using Microsoft.EntityFrameworkCore;
using TP_DisenoDB.Application.Interfaces;
using TP_DisenoDB.Domain.Entities;
using TP_DisenoDB.Infrastructure.Relational.DbContext;

namespace TP_DisenoDB.Infrastructure.Relational.Data;

public class MySqlDbSeeder : IDbSeeder
{
    private readonly MySqlDbContext _context;

    public MySqlDbSeeder(MySqlDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Solo sembramos si la base está vacía
        if (await _context.Banks.AnyAsync()) return;

        // 1. Bancos
        var bankGalicia = new Bank { Name = "Banco Galicia" };
        var bankSantander = new Bank { Name = "Banco Santander" };
        var bankNacion = new Bank { Name = "Banco Nación" };
        _context.Banks.AddRange(bankGalicia, bankSantander, bankNacion);

        // 2. Titulares
        var holder1 = new CardHolder { Name = "Juan", LastName = "Perez", Dni = "11111111" };
        var holder2 = new CardHolder { Name = "Maria", LastName = "Garcia", Dni = "22222222" };
        var holder3 = new CardHolder { Name = "Carlos", LastName = "Lopez", Dni = "33333333" };
        _context.CardHolders.AddRange(holder1, holder2, holder3);

        // 3. Tarjetas
        var card1 = new Card { CardNumber = "4545-1234-5678-9012", IssueDate = DateTime.Now.AddYears(-6), ExpiryDate = DateTime.Now.AddYears(2), Bank = bankGalicia, CardHolder = holder1 };
        var card2 = new Card { CardNumber = "5454-9876-5432-1098", IssueDate = DateTime.Now.AddYears(-2), ExpiryDate = DateTime.Now.AddYears(3), Bank = bankSantander, CardHolder = holder2 };
        var card3 = new Card { CardNumber = "3737-1111-2222-3333", IssueDate = DateTime.Now.AddYears(-1), ExpiryDate = DateTime.Now.AddYears(4), Bank = bankGalicia, CardHolder = holder3 };
        _context.Cards.AddRange(card1, card2, card3);

        // 4. Promociones
        var promoDesc = new DiscountPromotion 
        { 
            Code = "VERANO2025", 
            Percentage = 15, 
            StoreCuit = "20-12345678-9", 
            StartDate = DateTime.Now.AddMonths(-1), 
            EndDate = DateTime.Now.AddMonths(2), 
            Bank = bankGalicia,
            OnlyCash = false
        };
        var promoFinan = new FinancingPromotion 
        { 
            Code = "CUOTAS_FIJAS", 
            Installments = 6, 
            Interest = 5, 
            StoreCuit = "20-99999999-9", 
            StartDate = DateTime.Now.AddMonths(-1), 
            EndDate = DateTime.Now.AddMonths(1), 
            Bank = bankSantander 
        };
        _context.Promotions.AddRange(promoDesc, promoFinan);

        // 5. Compras
        // Compra contado
        var purchaseCash = new CashPurchase 
        { 
            StoreName = "Coto", StoreCuit = "20-12345678-9", PurchaseDate = DateTime.Now.AddDays(-5), 
            InitialValue = 10000, FinalValue = 8500, Card = card1, DiscountPercentage = 15 
        };
        purchaseCash.AppliedPromotions.Add(promoDesc);

        // Compra en cuotas
        var purchaseMonthly = new MonthlyPayments 
        { 
            StoreName = "Frávega", StoreCuit = "20-99999999-9", PurchaseDate = DateTime.Now.AddDays(-10), 
            InitialValue = 60000, FinalValue = 63000, Card = card2, InstallmentCount = 6, AdditionalPercentage = 5 
        };
        
        _context.Purchases.AddRange(purchaseCash, purchaseMonthly);

        // 6. Cuotas
        for (int i = 1; i <= 6; i++)
        {
            _context.Quotas.Add(new Quota { MonthlyPayments = purchaseMonthly, QuotaNumber = i, Value = 10500 });
        }

        await _context.SaveChangesAsync();
    }
}
