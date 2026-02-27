using System.Text.Json.Serialization;
namespace TP_DisenoDB.Domain.Entities;

public class Card
{
    public int Id { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    public int BankId { get; set; }
    
    [JsonIgnore]
    public virtual Bank? Bank { get; set; }

    public int CardHolderId { get; set; }
    
    [JsonIgnore]
    public virtual CardHolder? CardHolder { get; set; }

    [JsonIgnore]
    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
