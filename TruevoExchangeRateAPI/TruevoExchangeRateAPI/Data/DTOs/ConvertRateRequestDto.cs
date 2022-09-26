namespace TruevoExchangeRateAPI.Data.DTOs
{
    public class ConvertRateRequestDto
    {
        public decimal AmountToConvert { get; set; }
        public string? SourceCurrencyCode { get; set; }
        public string? TargetCurrencyCode { get; set; }
        public string? RateType { get; set; }
        public decimal BaseMargin { get; set; }
        public decimal TargetMargin { get; set; }
        public bool InverseMargin { get; set; }
    }
}
