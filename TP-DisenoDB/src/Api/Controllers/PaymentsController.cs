using Microsoft.AspNetCore.Mvc;
using TP_DisenoDB.Domain.Entities;
using TP_DisenoDB.Application.Interfaces;
using TP_DisenoDB.src.Api.Request;

namespace TP_DisenoDB.src.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("promotions/discount/{bankId}")]
    public async Task<IActionResult> AddDiscountPromotion(int bankId, [FromBody] DiscountPromotion promotion)
    {
        await _paymentService.AddDiscountPromotionAsync(bankId, promotion);
        return Ok();
    }

    [HttpPut("payments/{code}/due-dates")]
    public async Task<IActionResult> UpdateDueDates(string code, [FromBody] UpdateDueDatesRequest request)
    {
        await _paymentService.UpdatePaymentDueDatesAsync(code, request.DueDate1, request.DueDate2);
        return Ok();
    }

    [HttpGet("reports/monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int month, [FromQuery] int year)
    {
        var report = await _paymentService.GetPaymentReportAsync(month, year);
        if (report == null) return NotFound();
        return Ok(report);
    }

    [HttpGet("cards/old")]
    public async Task<IActionResult> GetOldCards()
    {
        var cards = await _paymentService.GetOldCardsAsync();
        return Ok(cards);
    }

    [HttpGet("purchases/{id}")]
    public async Task<IActionResult> GetPurchaseInfo(int id)
    {
        var purchase = await _paymentService.GetPurchaseWithQuotasAsync(id);
        if (purchase == null) return NotFound();
        return Ok(purchase);
    }

    [HttpDelete("promotions/{code}")]
    public async Task<IActionResult> DeletePromotion(string code)
    {
        await _paymentService.DeletePromotionByCodeAsync(code);
        return Ok();
    }

    [HttpGet("promotions/available")]
    public async Task<IActionResult> GetAvailablePromotions([FromQuery] PromotionSearchRequest request)
    {
        var promotions = await _paymentService.GetAvailablePromotionsAsync(request.StoreCuit, request.Start, request.End);
        return Ok(promotions);
    }

    [HttpGet("statistics/top-cardholders")]
    public async Task<IActionResult> GetTopCardHolders()
    {
        var names = await _paymentService.GetTop10CardHolderNamesAsync();
        return Ok(names);
    }

    [HttpGet("statistics/top-store")]
    public async Task<IActionResult> GetTopStore()
    {
        var store = await _paymentService.GetTopStoreAsync();
        return Ok(new { StoreName = store });
    }

    [HttpGet("statistics/top-bank")]
    public async Task<IActionResult> GetTopBank()
    {
        var bank = await _paymentService.GetTopBankAsync();
        return Ok(new { BankName = bank });
    }

    [HttpGet("statistics/clients-per-bank")]
    public async Task<IActionResult> GetClientsPerBank()
    {
        var counts = await _paymentService.GetClientCountPerBankAsync();
        return Ok(counts);
    }
}
