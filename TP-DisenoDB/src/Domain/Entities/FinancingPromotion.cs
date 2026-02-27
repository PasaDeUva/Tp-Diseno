using System.Text.Json.Serialization;
namespace TP_DisenoDB.Domain.Entities;

public class FinancingPromotion : Promotion
{
    public int Installments { get; set; }
    public decimal Interest { get; set; }
}
