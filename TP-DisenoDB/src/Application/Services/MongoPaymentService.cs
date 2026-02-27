using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TP_DisenoDB.Infrastructure.Mongo.Collections;
using TP_DisenoDB.Domain.Entities;
using TP_DisenoDB.Application.Interfaces;

namespace TP_DisenoDB.Application.Services;

public class MongoPaymentService : IPaymentService
{
    private readonly MongoContext _context;

    public MongoPaymentService(MongoContext context)
    {
        _context = context;
    }

    public async Task AddDiscountPromotionAsync(int bankId, DiscountPromotion promotion)
    {
        promotion.BankId = bankId;
        await _context.Promotions.InsertOneAsync(promotion);
    }

    public async Task UpdatePaymentDueDatesAsync(string internalCode, DateTime dueDate1, DateTime dueDate2)
    {
        var filter = Builders<Payment>.Filter.Eq(p => p.InternalCode, internalCode);
        var update = Builders<Payment>.Update
            .Set(p => p.DueDate1, dueDate1)
            .Set(p => p.DueDate2, dueDate2);
        await _context.Payments.UpdateOneAsync(filter, update);
    }

    public async Task<Payment?> GetPaymentReportAsync(int month, int year)
    {
        return await _context.Payments
            .Find(p => p.Month == month && p.Year == year)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Card>> GetOldCardsAsync()
    {
        var fiveYearsAgo = DateTime.Now.AddYears(-5);
        return await _context.Cards
            .Find(c => c.IssueDate < fiveYearsAgo)
            .ToListAsync();
    }

    public async Task<Purchase?> GetPurchaseWithQuotasAsync(int purchaseId)
    {
        return await _context.Purchases
            .Find(p => p.Id == purchaseId)
            .FirstOrDefaultAsync();
    }

    public async Task DeletePromotionByCodeAsync(string code)
    {
        var promotion = await _context.Promotions.Find(p => p.Code == code).FirstOrDefaultAsync();
        if (promotion != null)
        {
            // In Mongo, check if it's applied to any purchase
            // Using Builders for safety with polymorphic collections
            var filter = Builders<Purchase>.Filter.ElemMatch(p => p.AppliedPromotions, pr => pr.Code == code);
            var isApplied = await _context.Purchases.Find(filter).AnyAsync();
            if (isApplied)
            {
                return;
            }
            await _context.Promotions.DeleteOneAsync(p => p.Code == code);
        }
    }

    public async Task<IEnumerable<Promotion>> GetAvailablePromotionsAsync(string storeCuit, DateTime start, DateTime end)
    {
        return await _context.Promotions
            .Find(p => p.StoreCuit == storeCuit && p.StartDate <= end && p.EndDate >= start)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetTop10CardHolderNamesAsync()
    {
        var holders = await _context.CardHolders.Find(_ => true).ToListAsync();
        var cards = await _context.Cards.Find(_ => true).ToListAsync();
        var purchases = await _context.Purchases.Find(_ => true).ToListAsync();

        return holders
            .Select(h => new {
                Name = $"{h.Name} {h.LastName}",
                Total = purchases.Where(p => cards.Where(c => c.CardHolderId == h.Id).Select(c => c.Id).Contains(p.CardId)).Sum(p => p.FinalValue)
            })
            .OrderByDescending(x => x.Total)
            .Take(10)
            .Select(x => x.Name);
    }

    public async Task<string?> GetTopStoreAsync()
    {
        var result = await _context.Purchases.Aggregate()
            .Group(p => p.StoreName, g => new { Store = g.Key, Count = g.Count() })
            .SortByDescending(x => x.Count)
            .FirstOrDefaultAsync();
        return result?.Store;
    }

    public async Task<string?> GetTopBankAsync()
    {
        var cards = await _context.Cards.Find(_ => true).ToListAsync();
        var purchases = await _context.Purchases.Find(_ => true).ToListAsync();
        var banks = await _context.Banks.Find(_ => true).ToListAsync();

        return banks
            .Select(b => new {
                b.Name,
                Count = purchases.Count(p => cards.Where(c => c.BankId == b.Id).Select(c => c.Id).Contains(p.CardId))
            })
            .OrderByDescending(x => x.Count)
            .Select(x => x.Name)
            .FirstOrDefault();
    }

    public async Task<IDictionary<string, int>> GetClientCountPerBankAsync()
    {
        var banks = await _context.Banks.Find(_ => true).ToListAsync();
        var cards = await _context.Cards.Find(_ => true).ToListAsync();

        return banks.ToDictionary(
            b => b.Name,
            b => cards.Where(c => c.BankId == b.Id).Select(c => c.CardHolderId).Distinct().Count()
        );
    }
}
