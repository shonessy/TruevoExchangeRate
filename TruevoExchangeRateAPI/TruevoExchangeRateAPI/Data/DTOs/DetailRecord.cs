using System.Text.Json.Serialization;

namespace TruevoExchangeRateAPI.Data.DTOs
{
    public class DetailRecord
    {
        [JsonPropertyName("source_currency_code")]
        public int SourceCurrencyNumber { get; set; }

        [JsonPropertyName("source_currency_exponent")]
        public int SourceCurrencyExponent { get; set; }

        [JsonPropertyName("buy_currency_conversion_rate")]
        public decimal BuyCurrencyConversionRate { get; set; }

        [JsonPropertyName("mid_currency_conversion_rate")]
        public decimal MidCurrencyConversionRate { get; set; }

        [JsonPropertyName("sell_currency_conversion_rate")]
        public decimal SellCurrencyConversionRate { get; set; }
    }
}
