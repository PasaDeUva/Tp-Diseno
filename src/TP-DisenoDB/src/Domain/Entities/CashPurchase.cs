using System.Text.Json.Serialization;
namespace TP_DisenoDB.Domain.Entities;

public class CashPurchase : Purchase
{
    public decimal DiscountPercentage { get; set; }
    public int? PaymentId { get; set; }
    
    [JsonIgnore]
    public virtual Payment? Payment { get; set; }
}
