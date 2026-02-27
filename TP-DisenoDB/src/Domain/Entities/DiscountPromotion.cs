using System.Text.Json.Serialization;
namespace TP_DisenoDB.Domain.Entities;

public class DiscountPromotion : Promotion
{
    public decimal Percentage { get; set; }
    public decimal? MaxValue { get; set; }
    public bool OnlyCash { get; set; } // Based on "cuando el descuento especifique que es solo al contado"
}
