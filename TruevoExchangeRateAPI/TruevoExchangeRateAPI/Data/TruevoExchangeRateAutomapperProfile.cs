using AutoMapper;
using TruevoExchangeRateAPI.Data.DTOs;
using TruevoExchangeRateAPI.Data.Models;

namespace TruevoExchangeRateAPI.Data
{
    public class TruevoExchangeRateAutomapperProfile : Profile
    {
        public TruevoExchangeRateAutomapperProfile()
        {
            CreateMap<ExchangeRate, GetExchangeRateDto>().ReverseMap();
        }
    }
}
