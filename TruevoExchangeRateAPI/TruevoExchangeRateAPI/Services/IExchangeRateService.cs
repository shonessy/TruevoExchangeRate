using TruevoExchangeRateAPI.Data.Models;
using TruevoExchangeRateAPI.Data.DTOs;

namespace TruevoExchangeRateAPI.Services
{
    public interface IExchangeRateService
    {
        Task ImportExchangeRatesAsync(ImportExchangeRatesDto importExchangeRatesDto);

        Task<IEnumerable<ExchangeRate>> GetAllExchangeRatesAsync();

        Task<ConvertRateResponseDto> ConvertRateAsync(ConvertRateRequestDto convertRateRequest);
    }
}
