namespace TruevoExchangeRateAPI.Data.DTOs
{
    public static class RateType
    {
        public static readonly string BUY = "BUY";
        public static readonly string SELL = "SELL";

        public static bool IsValidRateType(string rateType)
        {
            return rateType.ToUpperInvariant() == BUY || rateType.ToUpperInvariant() == SELL;
        }
    }
}
