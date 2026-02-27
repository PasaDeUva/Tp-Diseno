using System.Text.Json.Serialization;
namespace TP_DisenoDB.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public string InternalCode { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime DueDate1 { get; set; }
    public DateTime DueDate2 { get; set; }
    public decimal IncrementPercentage1 { get; set; }
    public decimal IncrementPercentage2 { get; set; }

    [JsonIgnore]
    public virtual ICollection<Quota> Quotas { get; set; } = new List<Quota>();
    
    [JsonIgnore]
    public virtual ICollection<CashPurchase> CashPurchases { get; set; } = new List<CashPurchase>();
}
