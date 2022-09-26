namespace TruevoExchangeRateAPI.Data.DTOs
{
    public class GetExchangeRateDto
    {
        public int Id { get; set; }
        public int CurrencyNumber { get; set; }
        public string? CurrencyCode { get; set; }
        public int CurrencyExponent { get; set; }
        public decimal MastercardBuyRate { get; set; }
        public decimal TruevoBuyRate { get; set; }
        public decimal MastercardMidRate { get; set; }
        public decimal MastercardSellRate { get; set; }
        public decimal TruevoSellRate { get; set; }
        public DateTime ValidityDate { get; set; }
    }
}
