using System.Text.Json.Serialization;
namespace TP_DisenoDB.Domain.Entities;

public class MonthlyPayments : Purchase
{
    public decimal AdditionalPercentage { get; set; }
    public int InstallmentCount { get; set; }
    public virtual ICollection<Quota> Quotas { get; set; } = new List<Quota>();
}
