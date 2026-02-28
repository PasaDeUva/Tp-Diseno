using TP_DisenoDB.Domain.Entities;

namespace TP_DisenoDB.Application.Interfaces;

public interface IPaymentService
{
    // 1. Agregar una nueva promoción de tipo descuento a un banco dado
    Task AddDiscountPromotionAsync(int bankId, DiscountPromotion promotion);

    // 2. Editar las fecha de vencimiento de un pago con cierto código
    Task UpdatePaymentDueDatesAsync(string internalCode, DateTime dueDate1, DateTime dueDate2);

    // 3. Generar el total de pago de un mes dado, informando items
    Task<Payment?> GetPaymentReportAsync(int month, int year);

    // 4. Obtener el listado de tarjetas emitidas hace más de 5 años
    Task<IEnumerable<Card>> GetOldCardsAsync();

    // 5. Obtener la información de una compra, incluyendo cuotas si posee
    Task<Purchase?> GetPurchaseWithQuotasAsync(int purchaseId);

    // 6. Eliminar una promoción a traves de su código
    Task DeletePromotionByCodeAsync(string code);

    // 7. Obtener el listado de las promociones disponibles de un local entre dos fechas
    Task<IEnumerable<Promotion>> GetAvailablePromotionsAsync(string storeCuit, DateTime start, DateTime end);

    // 8. Obtener los nombres de los 10 titulares de tarjetas con mayor monto total en compras
    Task<IEnumerable<string>> GetTop10CardHolderNamesAsync();

    // 9. Obtener el nombre del local con mayor cantidad de compras registradas
    Task<string?> GetTopStoreAsync();

    // 10. Obtener el banco con mayor cantidad de compras realizadas con sus tarjetas
    Task<string?> GetTopBankAsync();

    // 11. Obtener el número de clientes de cada banco
    Task<IDictionary<string, int>> GetClientCountPerBankAsync();
}
