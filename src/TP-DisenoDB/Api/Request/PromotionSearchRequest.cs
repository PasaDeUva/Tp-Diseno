namespace TP_DisenoDB.src.Api.Request;

public class PromotionSearchRequest
{
    public string StoreCuit { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
