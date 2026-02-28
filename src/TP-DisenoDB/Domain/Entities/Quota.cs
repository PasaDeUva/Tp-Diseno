using System.Text.Json.Serialization;
namespace TP_DisenoDB.Domain.Entities;

public class Quota
{
    public int Id { get; set; }
    public int MonthlyPaymentsId { get; set; }
    
    [JsonIgnore]
    public virtual MonthlyPayments? MonthlyPayments { get; set; }
    
    public int QuotaNumber { get; set; }
    public decimal Value { get; set; }

    public int? PaymentId { get; set; }
    
    [JsonIgnore]
    public virtual Payment? Payment { get; set; }
}
