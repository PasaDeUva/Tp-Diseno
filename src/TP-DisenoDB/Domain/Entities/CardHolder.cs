using System.Text.Json.Serialization;
namespace TP_DisenoDB.Domain.Entities;

public class CardHolder
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    
    [JsonIgnore]
    public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
}
