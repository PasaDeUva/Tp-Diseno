using Microsoft.EntityFrameworkCore;
using TP_DisenoDB.Infrastructure.Relational.DbContext;
using TP_DisenoDB.Domain.Entities;
using TP_DisenoDB.Application.Interfaces;

namespace TP_DisenoDB.Application.Services;

public class MySqlPaymentService : IPaymentService
{
    private readonly MySqlDbContext _context;

    public MySqlPaymentService(MySqlDbContext context)
    {
        _context = context;
    }

    public async Task AddDiscountPromotionAsync(int bankId, DiscountPromotion promotion)
    {
        var bank = await _context.Banks.FindAsync(bankId);
        if (bank != null)
        {
            promotion.BankId = bankId;
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdatePaymentDueDatesAsync(string internalCode, DateTime dueDate1, DateTime dueDate2)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.InternalCode == internalCode);
        if (payment != null)
        {
            payment.DueDate1 = dueDate1;
            payment.DueDate2 = dueDate2;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Payment?> GetPaymentReportAsync(int month, int year)
    {
        return await _context.Payments
            .Include(p => p.Quotas)
                .ThenInclude(q => q.MonthlyPayments)
            .Include(p => p.CashPurchases)
            .FirstOrDefaultAsync(p => p.Month == month && p.Year == year);
    }

    public async Task<IEnumerable<Card>> GetOldCardsAsync()
    {
        var fiveYearsAgo = DateTime.Now.AddYears(-5);
        return await _context.Cards
            .Where(c => c.IssueDate < fiveYearsAgo)
            .ToListAsync();
    }

    public async Task<Purchase?> GetPurchaseWithQuotasAsync(int purchaseId)
    {
        var purchase = await _context.Purchases.FindAsync(purchaseId);
        if (purchase is MonthlyPayments monthly)
        {
            await _context.Entry(monthly).Collection(m => m.Quotas).LoadAsync();
        }
        return purchase;
    }

    public async Task DeletePromotionByCodeAsync(string code)
    {
        var promotion = await _context.Promotions
            .Include(p => p.AppliedToPurchases)
            .FirstOrDefaultAsync(p => p.Code == code);
        
        if (promotion != null)
        {
            if (promotion.AppliedToPurchases.Any())
            {
                return; 
            }
            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Promotion>> GetAvailablePromotionsAsync(string storeCuit, DateTime start, DateTime end)
    {
        return await _context.Promotions
            .Where(p => p.StoreCuit == storeCuit && p.StartDate <= end && p.EndDate >= start)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetTop10CardHolderNamesAsync()
    {
        return await _context.CardHolders
            .OrderByDescending(ch => ch.Cards.SelectMany(c => c.Purchases).Sum(p => p.FinalValue))
            .Take(10)
            .Select(ch => $"{ch.Name} {ch.LastName}")
            .ToListAsync();
    }

    public async Task<string?> GetTopStoreAsync()
    {
        return await _context.Purchases
            .GroupBy(p => p.StoreName)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetTopBankAsync()
    {
        return await _context.Banks
            .OrderByDescending(b => b.Cards.SelectMany(c => c.Purchases).Count())
            .Select(b => b.Name)
            .FirstOrDefaultAsync();
    }

    public async Task<IDictionary<string, int>> GetClientCountPerBankAsync()
    {
        return await _context.Banks
            .ToDictionaryAsync(b => b.Name, b => b.Cards.Select(c => c.CardHolderId).Distinct().Count());
    }
}
