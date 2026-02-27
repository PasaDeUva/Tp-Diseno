using System.Text.Json.Serialization;
namespace TP_DisenoDB.Domain.Entities;

public class Bank
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    [JsonIgnore]
    public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
    
    [JsonIgnore]
    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}
