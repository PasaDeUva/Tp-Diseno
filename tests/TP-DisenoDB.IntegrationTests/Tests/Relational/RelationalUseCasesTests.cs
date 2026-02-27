using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TP_DisenoDB.Domain.Entities;
using TP_DisenoDB.Infrastructure.Relational.DbContext;
using TP_DisenoDB.IntegrationTests.Fixtures;
using Xunit;

namespace TP_DisenoDB.IntegrationTests.Tests.Relational;

public class RelationalUseCasesTests : IClassFixture<RelationalTestFixture>
{
    private readonly HttpClient _client;
    private readonly RelationalTestFixture _fixture;

    public RelationalUseCasesTests(RelationalTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    private async Task SeedDataAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MySqlDbContext>();
        
        // Clean up
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // 1. Banks
        var bankA = new Bank { Name = "Banco Galicia" };
        var bankB = new Bank { Name = "Banco Santander" };
        context.Banks.AddRange(bankA, bankB);

        // 2. CardHolders
        var holder1 = new CardHolder { Name = "Juan", LastName = "Perez", Dni = "111" };
        var holder2 = new CardHolder { Name = "Maria", LastName = "Garcia", Dni = "222" };
        var holder3 = new CardHolder { Name = "Carlos", LastName = "Lopez", Dni = "333" };
        context.CardHolders.AddRange(holder1, holder2, holder3);

        // 3. Cards
        var card1 = new Card { CardNumber = "1234", IssueDate = DateTime.Now.AddYears(-6), ExpiryDate = DateTime.Now.AddYears(2), Bank = bankA, CardHolder = holder1 };
        var card2 = new Card { CardNumber = "5678", IssueDate = DateTime.Now.AddYears(-2), ExpiryDate = DateTime.Now.AddYears(3), Bank = bankB, CardHolder = holder2 };
        var card3 = new Card { CardNumber = "9012", IssueDate = DateTime.Now.AddYears(-1), ExpiryDate = DateTime.Now.AddYears(4), Bank = bankA, CardHolder = holder3 };
        context.Cards.AddRange(card1, card2, card3);

        // 4. Promotions
        var promo1 = new DiscountPromotion { Code = "PROMO_DESC", Percentage = 15, StoreCuit = "CUIT_SHOP_A", StartDate = DateTime.Now.AddDays(-10), EndDate = DateTime.Now.AddDays(10), Bank = bankA };
        context.Promotions.Add(promo1);

        // 5. Purchases
        // Purchase A: Cash (Juan)
        var p1 = new CashPurchase { StoreName = "Shop A", StoreCuit = "CUIT_SHOP_A", PurchaseDate = DateTime.Now, InitialValue = 1000, FinalValue = 850, Card = card1, DiscountPercentage = 15 };
        // Purchase B: Installments (Maria)
        var p2 = new MonthlyPayments { StoreName = "Shop B", StoreCuit = "CUIT_SHOP_B", PurchaseDate = DateTime.Now, InitialValue = 3000, FinalValue = 3300, Card = card2, InstallmentCount = 3, AdditionalPercentage = 10 };
        
        context.Purchases.AddRange(p1, p2);

        // 6. Quotas for p2
        for(int i=1; i<=3; i++) {
            context.Quotas.Add(new Quota { MonthlyPayments = p2, QuotaNumber = i, Value = 1100 });
        }

        // 7. A Payment (Statement) for current month
        var payment = new Payment { 
            InternalCode = "PAY_001", Month = DateTime.Now.Month, Year = DateTime.Now.Year, 
            DueDate1 = DateTime.Now.AddDays(5), DueDate2 = DateTime.Now.AddDays(10) 
        };
        payment.CashPurchases.Add(p1);
        // Add first quota of p2 to this payment
        context.Payments.Add(payment);

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Execute_All_Relational_Use_Cases()
    {
        await SeedDataAsync();

        // 1. Agregar una nueva promoción de tipo descuento
        var newPromo = new DiscountPromotion { Code = "NEW_PROMO", Percentage = 20, StoreCuit = "CUIT_X", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5) };
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
        Assert.Single(oldCards); // Solo card1
        Assert.Equal("1234", oldCards[0].CardNumber);

        // 5. Información de compra con cuotas
        var res5 = await _client.GetAsync("/api/Payments/purchases/2"); // ID 2 es la compra en cuotas
        var purchase = await res5.Content.ReadFromJsonAsync<MonthlyPayments>();
        Assert.NotNull(purchase);
        Assert.Equal(3, purchase.InstallmentCount);

        // 6. Eliminar promoción por código
        var res6 = await _client.DeleteAsync("/api/Payments/promotions/PROMO_DESC");
        if (!res6.IsSuccessStatusCode)
        {
            var content = await res6.Content.ReadAsStringAsync();
            throw new Exception($"Delete Promo failed: {res6.StatusCode}, {content}");
        }
        Assert.Equal(HttpStatusCode.OK, res6.StatusCode);

        // 7. Promociones disponibles local entre fechas
        var start = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        var end = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
        var res7 = await _client.GetAsync($"/api/Payments/promotions/available?storeCuit=CUIT_SHOP_A&start={start}&end={end}");
        Assert.Equal(HttpStatusCode.OK, res7.StatusCode);

        // 8. Top 10 titulares (en nuestro caso solo hay 3)
        var res8 = await _client.GetAsync("/api/Payments/statistics/top-cardholders");
        var topHolders = await res8.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotEmpty(topHolders);
        Assert.Contains("Maria Garcia", topHolders[0]); // Maria gastó 3300, Juan 850

        // 9. Local con mayor cantidad de compras
        var res9 = await _client.GetAsync("/api/Payments/statistics/top-store");
        var topStore = await res9.Content.ReadFromJsonAsync<dynamic>();
        // En nuestro seed hay 1 compra en Shop A y 1 en Shop B, devolverá una de las dos.

        // 10. Banco con mayor cantidad de compras
        var res10 = await _client.GetAsync("/api/Payments/statistics/top-bank");
        var topBank = await res10.Content.ReadFromJsonAsync<dynamic>();
        // Banco Santander (bankB) tiene la compra de Maria, Banco Galicia (bankA) la de Juan.

        // 11. Número de clientes de cada banco
        var res11 = await _client.GetAsync("/api/Payments/statistics/clients-per-bank");
        var clientsPerBank = await res11.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        Assert.True(clientsPerBank.ContainsKey("Banco Galicia"));
        Assert.Equal(2, clientsPerBank["Banco Galicia"]); // Juan y Carlos
    }
}
