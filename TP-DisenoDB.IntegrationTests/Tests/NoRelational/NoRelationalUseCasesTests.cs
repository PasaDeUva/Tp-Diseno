using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using TP_DisenoDB.Domain.Entities;
using TP_DisenoDB.Infrastructure.Mongo.Collections;
using TP_DisenoDB.IntegrationTests.Fixtures;
using Xunit;

namespace TP_DisenoDB.IntegrationTests.Tests.NoRelational;

public class NoRelationalUseCasesTests : IClassFixture<NoRelationalTestFixture>
{
    private readonly HttpClient _client;
    private readonly NoRelationalTestFixture _fixture;

    public NoRelationalUseCasesTests(NoRelationalTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    private async Task SeedDataAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MongoContext>();
        
        // Clean up
        await context.Banks.Database.DropCollectionAsync("Banks");
        await context.CardHolders.Database.DropCollectionAsync("CardHolders");
        await context.Cards.Database.DropCollectionAsync("Cards");
        await context.Purchases.Database.DropCollectionAsync("Purchases");
        await context.Promotions.Database.DropCollectionAsync("Promotions");
        await context.Payments.Database.DropCollectionAsync("Payments");

        // 1. Banks
        var bankA = new Bank { Id = 1, Name = "Banco Galicia" };
        var bankB = new Bank { Id = 2, Name = "Banco Santander" };
        await context.Banks.InsertManyAsync(new[] { bankA, bankB });

        // 2. CardHolders
        var holder1 = new CardHolder { Id = 1, Name = "Juan", LastName = "Perez", Dni = "111" };
        var holder2 = new CardHolder { Id = 2, Name = "Maria", LastName = "Garcia", Dni = "222" };
        var holder3 = new CardHolder { Id = 3, Name = "Carlos", LastName = "Lopez", Dni = "333" };
        await context.CardHolders.InsertManyAsync(new[] { holder1, holder2, holder3 });

        // 3. Cards
        var card1 = new Card { Id = 1, CardNumber = "1234", IssueDate = DateTime.Now.AddYears(-6), ExpiryDate = DateTime.Now.AddYears(2), BankId = 1, CardHolderId = 1 };
        var card2 = new Card { Id = 2, CardNumber = "5678", IssueDate = DateTime.Now.AddYears(-2), ExpiryDate = DateTime.Now.AddYears(3), BankId = 2, CardHolderId = 2 };
        var card3 = new Card { Id = 3, CardNumber = "9012", IssueDate = DateTime.Now.AddYears(-1), ExpiryDate = DateTime.Now.AddYears(4), BankId = 1, CardHolderId = 3 };
        await context.Cards.InsertManyAsync(new[] { card1, card2, card3 });

        // 4. Promotions
        var promo1 = new DiscountPromotion { Id = 1, Code = "PROMO_DESC", Percentage = 15, StoreCuit = "CUIT_SHOP_A", StartDate = DateTime.Now.AddDays(-10), EndDate = DateTime.Now.AddDays(10), BankId = 1 };
        await context.Promotions.InsertOneAsync(promo1);

        // 5. Purchases
        var p1 = new CashPurchase { Id = 1, StoreName = "Shop A", StoreCuit = "CUIT_SHOP_A", PurchaseDate = DateTime.Now, InitialValue = 1000, FinalValue = 850, CardId = 1, DiscountPercentage = 15 };
        var p2 = new MonthlyPayments { Id = 2, StoreName = "Shop B", StoreCuit = "CUIT_SHOP_B", PurchaseDate = DateTime.Now, InitialValue = 3000, FinalValue = 3300, CardId = 2, InstallmentCount = 3, AdditionalPercentage = 10 };
        
        // Quotas are embedded in MonthlyPayments or in Quotas collection? 
        // In our MongoService, we assumed they might be different. Let's see.
        // Actually, in the models they are a collection.
        p2.Quotas = new List<Quota> {
            new Quota { Id = 1, MonthlyPaymentsId = 2, QuotaNumber = 1, Value = 1100 },
            new Quota { Id = 2, MonthlyPaymentsId = 2, QuotaNumber = 2, Value = 1100 },
            new Quota { Id = 3, MonthlyPaymentsId = 2, QuotaNumber = 3, Value = 1100 }
        };

        await context.Purchases.InsertManyAsync(new Purchase[] { p1, p2 });

        // 7. A Payment (Statement) for current month
        var payment = new Payment { 
            Id = 1, InternalCode = "PAY_001", Month = DateTime.Now.Month, Year = DateTime.Now.Year, 
            DueDate1 = DateTime.Now.AddDays(5), DueDate2 = DateTime.Now.AddDays(10) 
        };
        // In Mongo implementation, we'll see if it works with references or embedded
        await context.Payments.InsertOneAsync(payment);
    }

    [Fact]
    public async Task Execute_All_NoRelational_Use_Cases()
    {
        await SeedDataAsync();

        // 1. Agregar una nueva promoción de tipo descuento
        var newPromo = new DiscountPromotion { Id = 2, Code = "NEW_PROMO", Percentage = 20, StoreCuit = "CUIT_X", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5) };
        var res1 = await _client.PostAsJsonAsync("/api/Payments/promotions/discount/1", newPromo);
        Assert.Equal(HttpStatusCode.OK, res1.StatusCode);

        // 2. Editar fecha de vencimiento
        var updateReq = new { DueDate1 = DateTime.Now.AddDays(15), DueDate2 = DateTime.Now.AddDays(20) };
        var res2 = await _client.PutAsJsonAsync("/api/Payments/payments/PAY_001/due-dates", updateReq);
        Assert.Equal(HttpStatusCode.OK, res2.StatusCode);

        // 3. Generar total de pago de un mes (reporte)
        var res3 = await _client.GetAsync($"/api/Payments/reports/monthly?month={DateTime.Now.Month}&year={DateTime.Now.Year}");
        var report = await res3.Content.ReadFromJsonAsync<Payment>();
        Assert.NotNull(report);
        Assert.Equal("PAY_001", report.InternalCode);

        // 4. Tarjetas emitidas hace más de 5 años
        var res4 = await _client.GetAsync("/api/Payments/cards/old");
        var oldCards = await res4.Content.ReadFromJsonAsync<List<Card>>();
        Assert.NotNull(oldCards);
        Assert.Contains(oldCards, c => c.CardNumber == "1234");

        // 5. Información de compra con cuotas
        var res5 = await _client.GetAsync("/api/Payments/purchases/2"); 
        var purchase = await res5.Content.ReadFromJsonAsync<MonthlyPayments>();
        Assert.NotNull(purchase);
        Assert.Equal(3, purchase.InstallmentCount);

        // 6. Eliminar promoción por código
        // In our MongoService, it checks if it's applied to any purchase.
        // promo1 is NOT applied to any purchase in the 'AppliedPromotions' list of p1 in our seed (p1 has null/empty list)
        var res6 = await _client.DeleteAsync("/api/Payments/promotions/PROMO_DESC");
        Assert.Equal(HttpStatusCode.OK, res6.StatusCode);

        // 7. Promociones disponibles local entre fechas
        var start = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        var end = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
        var res7 = await _client.GetAsync($"/api/Payments/promotions/available?storeCuit=CUIT_SHOP_A&start={start}&end={end}");
        Assert.Equal(HttpStatusCode.OK, res7.StatusCode);

        // 8. Top 10 titulares
        var res8 = await _client.GetAsync("/api/Payments/statistics/top-cardholders");
        var topHolders = await res8.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotEmpty(topHolders);

        // 9. Local con mayor cantidad de compras
        var res9 = await _client.GetAsync("/api/Payments/statistics/top-store");
        Assert.Equal(HttpStatusCode.OK, res9.StatusCode);

        // 10. Banco con mayor cantidad de compras
        var res10 = await _client.GetAsync("/api/Payments/statistics/top-bank");
        Assert.Equal(HttpStatusCode.OK, res10.StatusCode);

        // 11. Número de clientes de cada banco
        var res11 = await _client.GetAsync("/api/Payments/statistics/clients-per-bank");
        var clientsPerBank = await res11.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        Assert.NotNull(clientsPerBank);
    }
}
