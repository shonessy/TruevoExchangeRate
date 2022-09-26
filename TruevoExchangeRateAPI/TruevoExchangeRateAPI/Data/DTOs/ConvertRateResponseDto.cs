namespace TruevoExchangeRateAPI.Data.DTOs
{
    public class ConvertRateResponseDto : ConvertRateRequestDto
    {
        public decimal ConvertedAmount { get; set; }

        public ConvertRateResponseDto(ConvertRateRequestDto convertRateRequest)
        {
            AmountToConvert = convertRateRequest.AmountToConvert;
            SourceCurrencyCode = convertRateRequest.SourceCurrencyCode;
            TargetCurrencyCode = convertRateRequest.TargetCurrencyCode;
            RateType = convertRateRequest.RateType;
            BaseMargin = convertRateRequest.BaseMargin;
            TargetMargin = convertRateRequest.TargetMargin;
            InverseMargin = convertRateRequest.InverseMargin;
        }
    }
}
