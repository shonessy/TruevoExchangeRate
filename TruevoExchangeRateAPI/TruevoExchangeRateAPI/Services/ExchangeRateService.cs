using TruevoExchangeRateAPI.Data.Repository;
using TruevoExchangeRateAPI.Data.Models;
using TruevoExchangeRateAPI.Data.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TruevoExchangeRateAPI.Data.Options;
using TruevoExchangeRateAPI.Data;

namespace TruevoExchangeRateAPI.Services
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IRepository<ExchangeRate, int> _exchangeRateRepository;
        private readonly ILogger<ExchangeRateService> _logger;
        private readonly ExchangeRatesSettingsOptions _exchangeRatesSettingsOptions;

        public ExchangeRateService(
            IRepository<ExchangeRate, int> exchangeRateRepository,
            ILogger<ExchangeRateService> logger,
            IOptions<ExchangeRatesSettingsOptions> exchangeRatesSettingsOptions
        ) {
            _exchangeRateRepository = exchangeRateRepository;
            _logger = logger;
            _exchangeRatesSettingsOptions = exchangeRatesSettingsOptions.Value;
        }

        /// <summary>
        /// Method that retrieves all exchange rates as stored in the database.
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<ExchangeRate>> GetAllExchangeRatesAsync()
        {
            return _exchangeRateRepository.GetAllAsync();
        }

        /// <summary>
        /// Metod imports exchange rates in the database.
        /// It performs transforation of the rates to the base currency defined in the application configuration.
        /// Prior to imprtion, method deletes all the existing exchange rates.
        /// If any error happens, method deletes all exchange rates imported prior the errror.
        /// </summary>
        /// <param name="importExchangeRatesDto">Exchange rates to be imported.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task ImportExchangeRatesAsync(ImportExchangeRatesDto importExchangeRatesDto)
        {
            var baseRate = importExchangeRatesDto.DetailRecords?.FirstOrDefault(x => x.SourceCurrencyNumber == _exchangeRatesSettingsOptions.BaseCurrencyNumber);
            if(baseRate == null)
            {
                throw new ArgumentException($"Import exchange rates do not contain data for base currency {_exchangeRatesSettingsOptions.BaseCurrencyCode}");
            }

            await _exchangeRateRepository.DeleteAllAsync();

            // could be done with batching, but since there are ~200 currencies, simple one by one insert is performed
            try
            {
                foreach(var conversionRate in importExchangeRatesDto.DetailRecords)
                {
                    var currencyCode = CurrencyCodeNumberConverter.GetCurrencyCode(conversionRate.SourceCurrencyNumber);
                    var isBaseCurrency = IsBaseCurrency(currencyCode);
                    var mastercardBuyRate = isBaseCurrency ? 1 : CalculateSourceToTargetExchangeRate(conversionRate.BuyCurrencyConversionRate, baseRate.BuyCurrencyConversionRate);
                    var nastercardSellRate = isBaseCurrency ? 1 : CalculateSourceToTargetExchangeRate(conversionRate.SellCurrencyConversionRate, baseRate.SellCurrencyConversionRate);
                    var mastercardMidRate = isBaseCurrency ? 1 : CalculateSourceToTargetExchangeRate(conversionRate.MidCurrencyConversionRate, baseRate.MidCurrencyConversionRate);
                    var rateToInsert = new ExchangeRate
                    {
                        CurrencyNumber = conversionRate.SourceCurrencyNumber,
                        CurrencyCode = currencyCode,
                        CurrencyExponent = conversionRate.SourceCurrencyExponent,
                        MastercardBuyRate = mastercardBuyRate,
                        TruevoBuyRate = isBaseCurrency ? mastercardBuyRate : ApplyLoadToExchangeRate(mastercardBuyRate, _exchangeRatesSettingsOptions.InitialLoadPercentage, RateType.BUY),
                        MastercardSellRate = nastercardSellRate,
                        TruevoSellRate = isBaseCurrency ? nastercardSellRate : ApplyLoadToExchangeRate(nastercardSellRate, _exchangeRatesSettingsOptions.InitialLoadPercentage, RateType.SELL),
                        MastercardMidRate = mastercardMidRate,
                        TruevoMidRate = isBaseCurrency ? mastercardMidRate : ApplyLoadToExchangeRate(mastercardMidRate, _exchangeRatesSettingsOptions.InitialLoadPercentage, RateType.BUY),
                        ValidityDate = DateTime.SpecifyKind(Convert.ToDateTime(importExchangeRatesDto.HeaderRecord.Date), DateTimeKind.Utc)
                    };
                    await _exchangeRateRepository.InsertAsync(rateToInsert);
                }
            }
            catch(Exception)
            {
                await _exchangeRateRepository.DeleteAllAsync();
                throw;
            }
        }

        /// <summary>
        /// Method checks if the provided currency is the base currency.
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        private bool IsBaseCurrency(string currency)
        {
            return currency.ToLowerInvariant() == _exchangeRatesSettingsOptions.BaseCurrencyCode.ToLowerInvariant();
        }

        /// <summary>
        /// Method calculates source to target currency exchange rate, 
        /// based on source to base and target to base currency exchange rates.
        /// </summary>
        /// <param name="sourceToBaseRate">Source to base currency exchange rate.</param>
        /// <param name="targetToBaseRate">Target to base currency exchnage rate.</param>
        /// <returns></returns>
        private decimal CalculateSourceToTargetExchangeRate(decimal sourceToBaseRate, decimal targetToBaseRate)
        {
            return (1 / sourceToBaseRate) * targetToBaseRate;
        }

        /// <summary>
        /// Method applies the load to the exchange rate.
        /// If the rate type is "SELL", load will be negative.
        /// Otherwise it will be positive.
        /// </summary>
        /// <param name="exchangeRate">Exchnage rate to which the load should be applied.</param>
        /// <param name="loadPercentage">Load percentage.</param>
        /// <param name="rateType">Rate type. Allowed values "BUY" and "SELL".</param>
        /// <param name="inverseMargin">Inverse margin flag. Default value is false.</param>
        /// <returns></returns>
        private decimal ApplyLoadToExchangeRate(decimal exchangeRate, decimal loadPercentage, string rateType, bool inverseMargin = false)
        {
            var sign = rateType == RateType.SELL ? -1 : 1;
            if (inverseMargin)
            {
                sign *= -1;
            }
            return exchangeRate * (1 + sign * loadPercentage / 100);
        }

        /// <summary>
        /// Method performs currency conversion.
        /// </summary>
        /// <param name="convertRateRequest">Convert rate request.</param>
        /// <returns></returns>
        public async Task<ConvertRateResponseDto> ConvertRateAsync(ConvertRateRequestDto convertRateRequest)
        {
            var ret = new ConvertRateResponseDto(convertRateRequest);
            var validation = ValidateConvertRateRequest(convertRateRequest);
            if (!validation.IsValid)
            {
                ret.ConvertedAmount = validation.ConvertedAmount;
                return ret;
            }

            var sourceAndTargetRates = await GetSourceToBaseAndTargetToBaseRatesAsync(convertRateRequest.SourceCurrencyCode, convertRateRequest.TargetCurrencyCode, convertRateRequest.RateType);

            var rate = CalculateSourceToTargetExchangeRate(sourceAndTargetRates.SourceToBaseRate, sourceAndTargetRates.TargetToBaseRate);
            if (!IsBaseCurrency(convertRateRequest.SourceCurrencyCode))
            {
                rate = ApplyLoadToExchangeRate(rate, convertRateRequest.BaseMargin, convertRateRequest.RateType, convertRateRequest.InverseMargin);
            }
            if (!IsBaseCurrency(convertRateRequest.TargetCurrencyCode))
            {
                rate = ApplyLoadToExchangeRate(rate, convertRateRequest.TargetMargin, convertRateRequest.RateType, convertRateRequest.InverseMargin);
            }

            ret.ConvertedAmount = decimal.Round(ret.AmountToConvert * rate, _exchangeRatesSettingsOptions.AmountRoundDecimalPlaces, MidpointRounding.AwayFromZero);
            return ret;
        }

        /// <summary>
        /// Method that validates convert rate request.
        /// Request is considered invalid if:
        /// <list type="bullet">
        /// <item>Amount to convert is 0. Converted amount is then 0.</item>
        /// <item>Source currency is equal to the base currency. Converted amount is then equal to the amount to convert.</item>
        /// <item>Invalid rate type. Valid rate types are "BUY" and "SELL". Converted amount is then 0.</item>
        /// </list>
        /// </summary>
        /// <param name="convertRateRequest"></param>
        /// <returns></returns>
        private (bool IsValid, decimal ConvertedAmount) ValidateConvertRateRequest(ConvertRateRequestDto convertRateRequest)
        {
            if (convertRateRequest.AmountToConvert == 0)
            {
                return (false, 0);
            }
            if (convertRateRequest.SourceCurrencyCode == convertRateRequest.TargetCurrencyCode)
            {
                return (false, convertRateRequest.AmountToConvert);
            }
            if (!RateType.IsValidRateType(convertRateRequest.RateType))
            {
                return (false, 0);
            }

            return (true, 0);
        }

        /// <summary>
        /// Method returns source to base and target to base currency exchange rates
        /// based on the currency codes and rate type.
        /// </summary>
        /// <param name="sourceCurrencyCode">Source curreny exchange rate.</param>
        /// <param name="targetCurrencyCode">Target currency exchange rate.</param>
        /// <param name="rateType">Rate type.</param>
        /// <returns></returns>
        private async Task<(decimal SourceToBaseRate, decimal TargetToBaseRate)> GetSourceToBaseAndTargetToBaseRatesAsync(string sourceCurrencyCode, string targetCurrencyCode, string rateType)
        {
            var sourceRateRecord = await GetExchangeRateByCurrencyCodeAsync(sourceCurrencyCode);
            var targetRateRecord = await GetExchangeRateByCurrencyCodeAsync(targetCurrencyCode);
            if(rateType == RateType.SELL)
            {
                return (sourceRateRecord.TruevoSellRate, targetRateRecord.TruevoSellRate);
            }
            else if (rateType == RateType.BUY)
            {
                return (sourceRateRecord.TruevoBuyRate, targetRateRecord.TruevoBuyRate);
            }
            else
            {
                return (sourceRateRecord.TruevoMidRate, targetRateRecord.TruevoMidRate);
            }
        }

        /// <summary>
        /// Method returns the exchange rate by its currency code.
        /// </summary>
        /// <param name="currencyCode">Currency code.</param>
        /// <returns></returns>
        /// <throws>InvalidOperationException if the rate is not present in the database.</throws>
        private async Task<ExchangeRate> GetExchangeRateByCurrencyCodeAsync(string currencyCode)
        {
            currencyCode = currencyCode.ToUpperInvariant();
            return await _exchangeRateRepository
                .GetQueryable()
                .Where(x => x.CurrencyCode == currencyCode)
                .SingleAsync();
        }
    }
}
