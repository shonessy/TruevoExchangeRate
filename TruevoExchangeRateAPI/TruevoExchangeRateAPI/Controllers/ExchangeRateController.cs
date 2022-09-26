using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TruevoExchangeRateAPI.Data;
using TruevoExchangeRateAPI.Data.DTOs;
using TruevoExchangeRateAPI.Services;

namespace TruevoExchangeRateAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExchangeRateController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IMapper _mapper;

        public ExchangeRateController(
            IExchangeRateService exchangeRateService,
            IMapper mapper
        ) {
            _exchangeRateService = exchangeRateService;
            _mapper = mapper;
        }


        [HttpGet("getAll")]
        public async Task<IEnumerable<GetExchangeRateDto>> GetAll()
        {
            return _mapper.Map<IEnumerable<GetExchangeRateDto>>(await _exchangeRateService.GetAllExchangeRatesAsync());
        }


        [HttpGet("convert")]
        public async Task<ConvertRateResponseDto> Convert(decimal amount, string sourceCurrency, string targetCurrency, string rateType, decimal baseMarginPercentage = 0, decimal targetMarginPercentage = 0, bool inverseMargin = false)
        {
            if(!CurrencyCodeNumberConverter.IsValidCurrency(sourceCurrency))
            {
                throw new ArgumentException("Provided source currency is not valid.");
            }

            if (!CurrencyCodeNumberConverter.IsValidCurrency(targetCurrency))
            {
                throw new ArgumentException("Provided target currency is not valid.");
            }

            if (!RateType.IsValidRateType(rateType))
            {
                throw new ArgumentException("Provided rate type is not valid.");
            }

            try
            {
                return await _exchangeRateService.ConvertRateAsync(new ConvertRateRequestDto
                {
                    AmountToConvert = Math.Abs(amount),
                    SourceCurrencyCode = CurrencyCodeNumberConverter.GetCurrencyCode(sourceCurrency),
                    TargetCurrencyCode = CurrencyCodeNumberConverter.GetCurrencyCode(targetCurrency),
                    RateType = rateType.ToUpperInvariant(),
                    BaseMargin = baseMarginPercentage,
                    TargetMargin = targetMarginPercentage,
                    InverseMargin = inverseMargin
                });
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException("Unsupported base or target currency");
            }
        }


        [HttpPost("import")]
        public async Task Import([FromBody] ImportExchangeRatesDto importExchangeRatesDto)
        {
            await _exchangeRateService.ImportExchangeRatesAsync(importExchangeRatesDto);
        }
    }
}