namespace TruevoExchangeRateAPI.Data.Options
{
    public class ExchangeRatesSettingsOptions
    {
        public const string ExchangeRatesSettings = "ExchangeRatesSettings";

        public string BaseCurrencyCode { get; set; } = String.Empty;
        public int BaseCurrencyNumber { get; set; }
        public decimal InitialLoadPercentage { get; set; }
        public int AmountRoundDecimalPlaces { get; set; }
    }
}
