using MongoDB.Driver;
using TP_DisenoDB.Application.Interfaces;
using TP_DisenoDB.Domain.Entities;
using TP_DisenoDB.Infrastructure.Mongo.Collections;

namespace TP_DisenoDB.Infrastructure.Mongo.Data;

public class MongoDbSeeder : IDbSeeder
{
    private readonly MongoContext _context;

    public MongoDbSeeder(MongoContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var anyBank = await _context.Banks.Find(_ => true).AnyAsync();
        if (anyBank) 
        {
            Console.WriteLine("[MongoDbSeeder] Database already contains data. Skipping seeding.");
            return;
        }

        Console.WriteLine("[MongoDbSeeder] Seeding database...");
        // 1. Bancos
        var bankGalicia = new Bank { Id = 1, Name = "Banco Galicia" };
        var bankSantander = new Bank { Id = 2, Name = "Banco Santander" };
        await _context.Banks.InsertManyAsync(new[] { bankGalicia, bankSantander });

        // 2. Titulares
        var holder1 = new CardHolder { Id = 1, Name = "Juan", LastName = "Perez", Dni = "11111111" };
        var holder2 = new CardHolder { Id = 2, Name = "Maria", LastName = "Garcia", Dni = "22222222" };
        await _context.CardHolders.InsertManyAsync(new[] { holder1, holder2 });

        // 3. Tarjetas
        var card1 = new Card { Id = 1, CardNumber = "4545-1234-5678-9012", IssueDate = DateTime.Now.AddYears(-6), ExpiryDate = DateTime.Now.AddYears(2), BankId = 1, CardHolderId = 1 };
        var card2 = new Card { Id = 2, CardNumber = "5454-9876-5432-1098", IssueDate = DateTime.Now.AddYears(-2), ExpiryDate = DateTime.Now.AddYears(3), BankId = 2, CardHolderId = 2 };
        await _context.Cards.InsertManyAsync(new[] { card1, card2 });

        // 4. Promociones
        var promoDesc = new DiscountPromotion 
        { 
            Id = 1, Code = "VERANO2025", Percentage = 15, StoreCuit = "20-12345678-9", 
            StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(2), 
            BankId = 1, OnlyCash = false 
        };
        await _context.Promotions.InsertOneAsync(promoDesc);

        // 5. Compras
        var purchaseCash = new CashPurchase 
        { 
            Id = 1, StoreName = "Coto", StoreCuit = "20-12345678-9", PurchaseDate = DateTime.Now.AddDays(-5), 
            InitialValue = 10000, FinalValue = 8500, CardId = 1, DiscountPercentage = 15 
        };
        purchaseCash.AppliedPromotions.Add(promoDesc);

        var purchaseMonthly = new MonthlyPayments 
        { 
            Id = 2, StoreName = "Frávega", StoreCuit = "20-99999999-9", PurchaseDate = DateTime.Now.AddDays(-10), 
            InitialValue = 60000, FinalValue = 63000, CardId = 2, InstallmentCount = 3, AdditionalPercentage = 5 
        };
        purchaseMonthly.Quotas = new List<Quota>
        {
            new Quota { Id = 1, MonthlyPaymentsId = 2, QuotaNumber = 1, Value = 21000 },
            new Quota { Id = 2, MonthlyPaymentsId = 2, QuotaNumber = 2, Value = 21000 },
            new Quota { Id = 3, MonthlyPaymentsId = 2, QuotaNumber = 3, Value = 21000 }
        };

        await _context.Purchases.InsertManyAsync(new Purchase[] { purchaseCash, purchaseMonthly });
    }
}
