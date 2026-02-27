using System.Text.Json.Serialization;
namespace TP_DisenoDB.Domain.Entities;

public abstract class Promotion
{
    public int Id { get; set; }
    public string StoreCuit { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Code { get; set; } = string.Empty;

    public int BankId { get; set; }
    
    [JsonIgnore]
    public virtual Bank? Bank { get; set; }

    [JsonIgnore]
    public virtual ICollection<Purchase> AppliedToPurchases { get; set; } = new List<Purchase>();
}
