using System.Text.Json.Serialization;
namespace TP_DisenoDB.Domain.Entities;

public abstract class Purchase
{
    public int Id { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string StoreCuit { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public decimal InitialValue { get; set; }
    public decimal FinalValue { get; set; }

    [JsonIgnore]
    public virtual ICollection<Promotion> AppliedPromotions { get; set; } = new List<Promotion>();

    public int CardId { get; set; }
    
    [JsonIgnore]
    public virtual Card? Card { get; set; }
}
