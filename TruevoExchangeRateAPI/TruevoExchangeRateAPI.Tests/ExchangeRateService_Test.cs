using Microsoft.Extensions.Options;
using System.Reflection;
using TruevoExchangeRateAPI.Data;
using TruevoExchangeRateAPI.Data.DTOs;
using TruevoExchangeRateAPI.Data.Models;
using TruevoExchangeRateAPI.Data.Options;
using TruevoExchangeRateAPI.Data.Repository;
using TruevoExchangeRateAPI.Services;

namespace TruevoExchangeRateAPI.Tests;

public class ExchangeRateService_Test
{
    private readonly ExchangeRatesSettingsOptions _exchangeRatesSettingsOptions;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly IRepository<ExchangeRate, int> _exchangeRateRepository;

    public ExchangeRateService_Test(
        IOptions<ExchangeRatesSettingsOptions> exchangeRatesSettingsOptions, 
        IExchangeRateService exchangeRateService,
        TruevoExchangeRateDbContext context,
        IRepository<ExchangeRate, int> exchangeRateRepository
    ) {
        _exchangeRatesSettingsOptions = exchangeRatesSettingsOptions.Value;
        _exchangeRateService = exchangeRateService;
        _exchangeRateRepository = exchangeRateRepository;
        context.Database.EnsureCreated();
    }

    [Fact]
    public void IsBaseCurrency_Should_Return_True_On_Correct_Base_Currency()
    {
        //Arrange
        var method = _exchangeRateService.GetType().GetMethod("IsBaseCurrency", BindingFlags.Instance | BindingFlags.NonPublic);

        //Act
        var result = (bool)method.Invoke(_exchangeRateService, new[] { _exchangeRatesSettingsOptions.BaseCurrencyCode });

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void IsBaseCurrency_Should_Return_False_On_Wrong_Base_Currency()
    {
        //Arrange
        var method = _exchangeRateService.GetType().GetMethod("IsBaseCurrency", BindingFlags.Instance | BindingFlags.NonPublic);

        //Act
        var result = (bool)method.Invoke(_exchangeRateService, new[] { "XXX" });

        //Assert
        Assert.False(result);
    }

    [Fact]
    public void CalculateSourceToTargetExchangeRate_Should_Return_Correct_Exchange_Rate()
    {
        // Arrange
        var sourceToBaseRate = 12.56m;
        var targetToBaseRate = 5.41m;
        var expectedRate = (1 / sourceToBaseRate) * targetToBaseRate;
        var method = _exchangeRateService.GetType().GetMethod("CalculateSourceToTargetExchangeRate", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var calculatedRate = (decimal)method.Invoke(_exchangeRateService, new object[] { sourceToBaseRate, targetToBaseRate });

        // Assert
        Assert.Equal(expectedRate, calculatedRate);
    }

    [Fact]
    public void ApplyLoadToExchangeRate_Should_Return_Correct_Sell_Exchange_Rate()
    {
        // Arrange
        var exchangeRate = 12.56m;
        var loadPercentage = 5.41m;
        var expectedRate = exchangeRate * (1 - loadPercentage / 100);
        var method = _exchangeRateService.GetType().GetMethod("ApplyLoadToExchangeRate", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var calculatedRate = (decimal)method.Invoke(_exchangeRateService, new object[] { exchangeRate, loadPercentage, RateType.SELL, false });

        // Assert
        Assert.Equal(expectedRate, calculatedRate);
    }

    [Fact]
    public void ApplyLoadToExchangeRate_Should_Return_Correct_Buy_Exchange_Rate()
    {
        // Arrange
        var exchangeRate = 12.56m;
        var loadPercentage = 5.41m;
        var expectedRate = exchangeRate * (1 + loadPercentage / 100);
        var method = _exchangeRateService.GetType().GetMethod("ApplyLoadToExchangeRate", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var calculatedRate = (decimal)method.Invoke(_exchangeRateService, new object[] { exchangeRate, loadPercentage, RateType.BUY, false });

        // Assert
        Assert.Equal(expectedRate, calculatedRate);
    }

    [Fact]
    public void ValidateConvertRateRequest_Should_Return_Invalid_On_AmountToConvert_0()
    {
        // Arrange
        var request = new ConvertRateRequestDto
        {
            AmountToConvert = 0
        };
        var method = _exchangeRateService.GetType().GetMethod("ValidateConvertRateRequest", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var status = (dynamic) method.Invoke(_exchangeRateService, new object[] { request });

        // Assert
        Assert.False((bool)status.Item1);
        Assert.Equal(0m, (decimal)status.Item2);
    }

    [Fact]
    public void ValidateConvertRateRequest_Should_Return_Invalid_On_Source_Currency_Equal_Target_Currency()
    {
        // Arrange
        var request = new ConvertRateRequestDto
        {
            AmountToConvert = 10m,
            SourceCurrencyCode = "USD",
            TargetCurrencyCode = "USD"
        };
        var method = _exchangeRateService.GetType().GetMethod("ValidateConvertRateRequest", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var status = (dynamic)method.Invoke(_exchangeRateService, new object[] { request });

        // Assert
        Assert.False((bool)status.Item1);
        Assert.Equal(request.AmountToConvert, (decimal)status.Item2);
    }

    [Fact]
    public void ValidateConvertRateRequest_Should_Return_Invalid_On_Invalid_Rate_Type()
    {
        // Arrange
        var request = new ConvertRateRequestDto
        {
            AmountToConvert = 10m,
            SourceCurrencyCode = "USD",
            TargetCurrencyCode = "EUR",
            RateType = "Test"
        };
        var method = _exchangeRateService.GetType().GetMethod("ValidateConvertRateRequest", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var status = (dynamic)method.Invoke(_exchangeRateService, new object[] { request });

        // Assert
        Assert.False((bool)status.Item1);
        Assert.Equal(0m, (decimal)status.Item2);
    }

    [Fact]
    public void ValidateConvertRateRequest_Should_Return_Valid_On_Correct_Request()
    {
        // Arrange
        var request = new ConvertRateRequestDto
        {
            AmountToConvert = 10m,
            SourceCurrencyCode = "USD",
            TargetCurrencyCode = "EUR",
            RateType = "BUY"
        };
        var method = _exchangeRateService.GetType().GetMethod("ValidateConvertRateRequest", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var status = (dynamic)method.Invoke(_exchangeRateService, new object[] { request });

        // Assert
        Assert.True((bool)status.Item1);
        Assert.Equal(0m, (decimal)status.Item2);
    }

    [Fact]
    public async Task GetAllExchangeRatesAsync_Should_Return_All_Existing_Exchange_Rates()
    {
        // Arrange
        var ratesToInsert = new List<ExchangeRate>
        {
            new ExchangeRate
            {
                CurrencyNumber = 123,
                CurrencyCode = "EUR",
                CurrencyExponent = 2,
                MastercardBuyRate = 12.56m,
                TruevoBuyRate = 12.75m,
                MastercardSellRate = 15.41m,
                TruevoSellRate = 15.25m,
                MastercardMidRate = 2.61m,
                ValidityDate = DateTime.UtcNow
            },
            new ExchangeRate
            {
                CurrencyNumber = 124,
                CurrencyCode = "USD",
                CurrencyExponent = 2,
                MastercardBuyRate = 11.56m,
                TruevoBuyRate = 11.75m,
                MastercardSellRate = 11.41m,
                TruevoSellRate = 11.25m,
                MastercardMidRate = 1.61m,
                ValidityDate = DateTime.UtcNow
            }
        };
        foreach(var rate in ratesToInsert)
        {
            await _exchangeRateRepository.InsertAsync(rate);
        }

        // Act
        var rates = (await _exchangeRateService.GetAllExchangeRatesAsync()).ToList();

        // Assert
        Assert.Equal(2, rates.Count());

        Assert.Equal(ratesToInsert[0].CurrencyNumber, rates[0].CurrencyNumber);
        Assert.Equal(ratesToInsert[0].CurrencyCode, rates[0].CurrencyCode);
        Assert.Equal(ratesToInsert[0].CurrencyExponent, rates[0].CurrencyExponent);
        Assert.Equal(ratesToInsert[0].MastercardBuyRate, rates[0].MastercardBuyRate);
        Assert.Equal(ratesToInsert[0].TruevoBuyRate, rates[0].TruevoBuyRate);
        Assert.Equal(ratesToInsert[0].MastercardSellRate, rates[0].MastercardSellRate);
        Assert.Equal(ratesToInsert[0].TruevoSellRate, rates[0].TruevoSellRate);
        Assert.Equal(ratesToInsert[0].MastercardMidRate, rates[0].MastercardMidRate);
        Assert.Equal(ratesToInsert[0].ValidityDate, rates[0].ValidityDate);

        Assert.Equal(ratesToInsert[1].CurrencyNumber, rates[1].CurrencyNumber);
        Assert.Equal(ratesToInsert[1].CurrencyCode, rates[1].CurrencyCode);
        Assert.Equal(ratesToInsert[1].CurrencyExponent, rates[1].CurrencyExponent);
        Assert.Equal(ratesToInsert[1].MastercardBuyRate, rates[1].MastercardBuyRate);
        Assert.Equal(ratesToInsert[1].TruevoBuyRate, rates[1].TruevoBuyRate);
        Assert.Equal(ratesToInsert[1].MastercardSellRate, rates[1].MastercardSellRate);
        Assert.Equal(ratesToInsert[1].TruevoSellRate, rates[1].TruevoSellRate);
        Assert.Equal(ratesToInsert[1].MastercardMidRate, rates[1].MastercardMidRate);
        Assert.Equal(ratesToInsert[1].ValidityDate, rates[1].ValidityDate);
    }

    [Fact]
    public async Task ImportExchangeRatesAsync_Should_Throw_Excepton_On_Empty_Details_Record()
    {
        // Arrange
        var import = new ImportExchangeRatesDto();

        // Act
        Task task() => _exchangeRateService.ImportExchangeRatesAsync(import);

        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(task);
        Assert.Equal("Import exchange rates do not contain data for base currency EUR", exception.Message);
    }

    [Fact]
    public async Task ImportExchangeRatesAsync_Should_Set_Exchange_Rates_1_For_Base_Currency()
    {
        // Arrange
        var ratesToImport = new ImportExchangeRatesDto
        {
            HeaderRecord = new HeaderRecord { Date = "2017-10-21 14:00:19" },
            DetailRecords = new List<DetailRecord>()
            {
                new DetailRecord
                {
                    SourceCurrencyNumber = _exchangeRatesSettingsOptions.BaseCurrencyNumber,
                    SourceCurrencyExponent = 2,
                    BuyCurrencyConversionRate = 12.56m,
                    MidCurrencyConversionRate = 15.56m,
                    SellCurrencyConversionRate = 17.56m
                }
            }
        };

        // Act
        await _exchangeRateService.ImportExchangeRatesAsync(ratesToImport);

        // Assert
        var importedRates = (await _exchangeRateService.GetAllExchangeRatesAsync()).ToList();

        Assert.Single(importedRates);
        Assert.Equal(ratesToImport.DetailRecords.ElementAt(0).SourceCurrencyNumber, importedRates[0].CurrencyNumber);
        Assert.Equal(_exchangeRatesSettingsOptions.BaseCurrencyCode, importedRates[0].CurrencyCode);
        Assert.Equal(ratesToImport.DetailRecords.ElementAt(0).SourceCurrencyExponent, importedRates[0].CurrencyExponent);
        Assert.Equal(1m, importedRates[0].MastercardBuyRate);
        Assert.Equal(1m, importedRates[0].TruevoBuyRate);
        Assert.Equal(1m, importedRates[0].MastercardSellRate);
        Assert.Equal(1m, importedRates[0].TruevoSellRate);
        Assert.Equal(1m, importedRates[0].MastercardMidRate);
        Assert.Equal(Convert.ToDateTime(ratesToImport.HeaderRecord.Date), importedRates[0].ValidityDate);
    }

    [Fact]
    public async Task ImportExchangeRatesAsync_Should_Correctly_Set_Exchange_Rates_For_Non_Base_Currency()
    {
        // Arrange
        var ratesToImport = new ImportExchangeRatesDto
        {
            HeaderRecord = new HeaderRecord { Date = "2017-10-21 14:00:19" },
            DetailRecords = new List<DetailRecord>()
            {
                new DetailRecord
                {
                    SourceCurrencyNumber = 840,                 // USD
                    SourceCurrencyExponent = 2,
                    BuyCurrencyConversionRate = 1m,
                    MidCurrencyConversionRate = 1m,
                    SellCurrencyConversionRate = 1m
                },
                new DetailRecord
                {
                    SourceCurrencyNumber = _exchangeRatesSettingsOptions.BaseCurrencyNumber,
                    SourceCurrencyExponent = 2,
                    BuyCurrencyConversionRate = 12.56m,
                    MidCurrencyConversionRate = 15.56m,
                    SellCurrencyConversionRate = 17.56m
                }
            }
        };

        // Act
        await _exchangeRateService.ImportExchangeRatesAsync(ratesToImport);

        // Assert
        var importedRates = (await _exchangeRateService.GetAllExchangeRatesAsync()).ToList();
        var usdRate = importedRates.Single(x => x.CurrencyCode == "USD");
        var eurRate = importedRates.Single(x => x.CurrencyCode == "EUR");
        var expectedMastercardBuyRate = (1 / ratesToImport.DetailRecords.ElementAt(0).BuyCurrencyConversionRate) * ratesToImport.DetailRecords.ElementAt(1).BuyCurrencyConversionRate;
        var expectedMastercardMidRate = (1 / ratesToImport.DetailRecords.ElementAt(0).MidCurrencyConversionRate) * ratesToImport.DetailRecords.ElementAt(1).MidCurrencyConversionRate;
        var expectedMastercardSellRate = (1 / ratesToImport.DetailRecords.ElementAt(0).SellCurrencyConversionRate) * ratesToImport.DetailRecords.ElementAt(1).SellCurrencyConversionRate;
        var expectedTruevoBuyRate = expectedMastercardBuyRate * (1 + _exchangeRatesSettingsOptions.InitialLoadPercentage / 100);
        var expectedTruevoSellRate = expectedMastercardSellRate * (1 - _exchangeRatesSettingsOptions.InitialLoadPercentage / 100);

        Assert.Equal(2, importedRates.Count());

        Assert.Equal(ratesToImport.DetailRecords.ElementAt(0).SourceCurrencyNumber, usdRate.CurrencyNumber);
        Assert.Equal("USD", usdRate.CurrencyCode);
        Assert.Equal(ratesToImport.DetailRecords.ElementAt(0).SourceCurrencyExponent, usdRate.CurrencyExponent);
        Assert.Equal(expectedMastercardBuyRate, usdRate.MastercardBuyRate);
        Assert.Equal(expectedTruevoBuyRate, usdRate.TruevoBuyRate);
        Assert.Equal(expectedMastercardSellRate, usdRate.MastercardSellRate);
        Assert.Equal(expectedTruevoSellRate, usdRate.TruevoSellRate);
        Assert.Equal(expectedMastercardMidRate, importedRates[0].MastercardMidRate);
        Assert.Equal(Convert.ToDateTime(ratesToImport.HeaderRecord.Date), importedRates[0].ValidityDate);
    }

    [Fact]
    public async Task GetExchangeRateByCurrencyCodeAsync_Should_Return_Correct_Curreny()
    {
        // Arrange
        var ratesToInsert = new List<ExchangeRate>
        {
            new ExchangeRate
            {
                CurrencyNumber = 123,
                CurrencyCode = "EUR",
                CurrencyExponent = 2,
                MastercardBuyRate = 12.56m,
                TruevoBuyRate = 12.75m,
                MastercardSellRate = 15.41m,
                TruevoSellRate = 15.25m,
                MastercardMidRate = 2.61m,
                ValidityDate = DateTime.UtcNow
            },
            new ExchangeRate
            {
                CurrencyNumber = 124,
                CurrencyCode = "USD",
                CurrencyExponent = 2,
                MastercardBuyRate = 11.56m,
                TruevoBuyRate = 11.75m,
                MastercardSellRate = 11.41m,
                TruevoSellRate = 11.25m,
                MastercardMidRate = 1.61m,
                ValidityDate = DateTime.UtcNow
            }
        };
        foreach (var rate in ratesToInsert)
        {
            await _exchangeRateRepository.InsertAsync(rate);
        }        
        var method = _exchangeRateService.GetType().GetMethod("GetExchangeRateByCurrencyCodeAsync", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var task = (Task)method.Invoke(_exchangeRateService, new object[] { "USD" });
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        var usdRate = (ExchangeRate)resultProperty.GetValue(task);

        // Assert
        Assert.Equal(ratesToInsert[1].CurrencyNumber, usdRate.CurrencyNumber);
        Assert.Equal(ratesToInsert[1].CurrencyCode, usdRate.CurrencyCode);
        Assert.Equal(ratesToInsert[1].CurrencyExponent, usdRate.CurrencyExponent);
        Assert.Equal(ratesToInsert[1].MastercardBuyRate, usdRate.MastercardBuyRate);
        Assert.Equal(ratesToInsert[1].TruevoBuyRate, usdRate.TruevoBuyRate);
        Assert.Equal(ratesToInsert[1].MastercardSellRate, usdRate.MastercardSellRate);
        Assert.Equal(ratesToInsert[1].TruevoSellRate, usdRate.TruevoSellRate);
        Assert.Equal(ratesToInsert[1].MastercardMidRate, usdRate.MastercardMidRate);
        Assert.Equal(ratesToInsert[1].ValidityDate, usdRate.ValidityDate);
    }

    [Fact]
    public async Task GetSourceToBaseAndTargetToBaseRatesAsync_Should_Return_Correct_Buy_Rates()
    {
        // Arrange
        var ratesToInsert = new List<ExchangeRate>
        {
            new ExchangeRate
            {
                CurrencyNumber = 123,
                CurrencyCode = "EUR",
                CurrencyExponent = 2,
                MastercardBuyRate = 12.56m,
                TruevoBuyRate = 12.75m,
                MastercardSellRate = 15.41m,
                TruevoSellRate = 15.25m,
                MastercardMidRate = 2.61m,
                ValidityDate = DateTime.UtcNow
            },
            new ExchangeRate
            {
                CurrencyNumber = 124,
                CurrencyCode = "USD",
                CurrencyExponent = 2,
                MastercardBuyRate = 11.56m,
                TruevoBuyRate = 11.75m,
                MastercardSellRate = 11.41m,
                TruevoSellRate = 11.25m,
                MastercardMidRate = 1.61m,
                ValidityDate = DateTime.UtcNow
            }
        };
        foreach (var rate in ratesToInsert)
        {
            await _exchangeRateRepository.InsertAsync(rate);
        }
        var method = _exchangeRateService.GetType().GetMethod("GetSourceToBaseAndTargetToBaseRatesAsync", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var task = (Task)method.Invoke(_exchangeRateService, new object[] { "EUR", "USD", RateType.BUY });
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        var result = (dynamic)resultProperty.GetValue(task);

        // Assert
        Assert.Equal(result.Item1, ratesToInsert[0].TruevoBuyRate);
        Assert.Equal(result.Item2, ratesToInsert[1].TruevoBuyRate);
    }

    [Fact]
    public async Task GetSourceToBaseAndTargetToBaseRatesAsync_Should_Return_Correct_Sell_Rates()
    {
        // Arrange
        var ratesToInsert = new List<ExchangeRate>
        {
            new ExchangeRate
            {
                CurrencyNumber = 123,
                CurrencyCode = "EUR",
                CurrencyExponent = 2,
                MastercardBuyRate = 12.56m,
                TruevoBuyRate = 12.75m,
                MastercardSellRate = 15.41m,
                TruevoSellRate = 15.25m,
                MastercardMidRate = 2.61m,
                ValidityDate = DateTime.UtcNow
            },
            new ExchangeRate
            {
                CurrencyNumber = 124,
                CurrencyCode = "USD",
                CurrencyExponent = 2,
                MastercardBuyRate = 11.56m,
                TruevoBuyRate = 11.75m,
                MastercardSellRate = 11.41m,
                TruevoSellRate = 11.25m,
                MastercardMidRate = 1.61m,
                ValidityDate = DateTime.UtcNow
            }
        };
        foreach (var rate in ratesToInsert)
        {
            await _exchangeRateRepository.InsertAsync(rate);
        }
        var method = _exchangeRateService.GetType().GetMethod("GetSourceToBaseAndTargetToBaseRatesAsync", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var task = (Task)method.Invoke(_exchangeRateService, new object[] { "EUR", "USD", RateType.SELL });
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        var result = (dynamic)resultProperty.GetValue(task);

        // Assert
        Assert.Equal(result.Item1, ratesToInsert[0].TruevoSellRate);
        Assert.Equal(result.Item2, ratesToInsert[1].TruevoSellRate);
    }

    [Fact]
    public async Task ConvertRateAsync_Should_Have_Converted_Amount_0_On_Amount_To_Convert_0()
    {
        // Arrange
        var input = new ConvertRateRequestDto
        {
            AmountToConvert = 0,
            SourceCurrencyCode = "USD",
            TargetCurrencyCode = "USD",
            RateType = RateType.BUY
        };

        // Act
        var result = await _exchangeRateService.ConvertRateAsync(input);

        // Assert
        Assert.Equal(input.AmountToConvert, result.AmountToConvert);
        Assert.Equal(input.SourceCurrencyCode, result.SourceCurrencyCode);
        Assert.Equal(input.TargetCurrencyCode, result.TargetCurrencyCode);
        Assert.Equal(input.AmountToConvert, result.ConvertedAmount);
        Assert.Equal(input.RateType, result.RateType);
    }

    [Fact]
    public async Task ConvertRateAsync_Should_Have_Converted_Amount_0_On_Invalid_Rate_Type()
    {
        // Arrange
        var input = new ConvertRateRequestDto
        {
            AmountToConvert = 10,
            SourceCurrencyCode = "EUR",
            TargetCurrencyCode = "USD",
            RateType = "TEST"
        };

        // Act
        var result = await _exchangeRateService.ConvertRateAsync(input);

        // Assert
        Assert.Equal(input.AmountToConvert, result.AmountToConvert);
        Assert.Equal(input.SourceCurrencyCode, result.SourceCurrencyCode);
        Assert.Equal(input.TargetCurrencyCode, result.TargetCurrencyCode);
        Assert.Equal(0, result.ConvertedAmount);
        Assert.Equal(input.RateType, result.RateType);
    }

    [Fact]
    public async Task ConvertRateAsync_Should_Have_Converted_Amount_Equal_Amount_To_Convert_On_Source_Currency_Equal_Target_Currency()
    {
        // Arrange
        var input = new ConvertRateRequestDto
        {
            AmountToConvert = 10,
            SourceCurrencyCode = "USD",
            TargetCurrencyCode = "USD",
            RateType = RateType.BUY
        };

        // Act
        var result = await _exchangeRateService.ConvertRateAsync(input);

        // Assert
        Assert.Equal(input.AmountToConvert, result.AmountToConvert);
        Assert.Equal(input.SourceCurrencyCode, result.SourceCurrencyCode);
        Assert.Equal(input.TargetCurrencyCode, result.TargetCurrencyCode);
        Assert.Equal(input.AmountToConvert, result.ConvertedAmount);
        Assert.Equal(input.RateType, result.RateType);
    }

    [Fact]
    public async Task ConvertRateAsync_Should_Calculate_Correct_Amout_From_Source_To_Base_Currency()
    {
        // Arrange
        var ratesToInsert = new List<ExchangeRate>
        {
            new ExchangeRate
            {
                CurrencyNumber = 123,
                CurrencyCode = "EUR",
                CurrencyExponent = 2,
                MastercardBuyRate = 1m,
                TruevoBuyRate = 1m,
                MastercardSellRate = 1m,
                TruevoSellRate = 1m,
                MastercardMidRate = 1m,
                ValidityDate = DateTime.UtcNow
            },
            new ExchangeRate
            {
                CurrencyNumber = 124,
                CurrencyCode = "USD",
                CurrencyExponent = 2,
                MastercardBuyRate = 11.56m,
                TruevoBuyRate = 11.75m,
                MastercardSellRate = 11.41m,
                TruevoSellRate = 11.25m,
                MastercardMidRate = 1.61m,
                ValidityDate = DateTime.UtcNow
            }
        };
        foreach (var rate in ratesToInsert)
        {
            await _exchangeRateRepository.InsertAsync(rate);
        }

        var input = new ConvertRateRequestDto
        {
            AmountToConvert = 10,
            SourceCurrencyCode = "USD",
            TargetCurrencyCode = "EUR",
            RateType = RateType.BUY,
            BaseMargin = 5,
            TargetMargin = 15
        };
        var method = _exchangeRateService.GetType().GetMethod("GetExchangeRateByCurrencyCodeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        var task = (Task)method.Invoke(_exchangeRateService, new object[] { "USD" });
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        var usdRate = (ExchangeRate)resultProperty.GetValue(task);
        var expectedConvertedAmount = input.AmountToConvert / usdRate.TruevoBuyRate;
        expectedConvertedAmount *= 1 + input.BaseMargin / 100;
        expectedConvertedAmount = decimal.Round(expectedConvertedAmount, _exchangeRatesSettingsOptions.AmountRoundDecimalPlaces, MidpointRounding.AwayFromZero);

        // Act
        var result = await _exchangeRateService.ConvertRateAsync(input);

        // Assert
        Assert.Equal(input.AmountToConvert, result.AmountToConvert);
        Assert.Equal(input.SourceCurrencyCode, result.SourceCurrencyCode);
        Assert.Equal(input.TargetCurrencyCode, result.TargetCurrencyCode);
        Assert.Equal(input.RateType, result.RateType);
        Assert.Equal(expectedConvertedAmount, result.ConvertedAmount);
    }

    [Fact]
    public async Task ConvertRateAsync_Should_Calculate_Correct_Amout_From_Source_To_Base_Currency_With_Inverse_Margin()
    {
        // Arrange
        var ratesToInsert = new List<ExchangeRate>
        {
            new ExchangeRate
            {
                CurrencyNumber = 123,
                CurrencyCode = "EUR",
                CurrencyExponent = 2,
                MastercardBuyRate = 1m,
                TruevoBuyRate = 1m,
                MastercardSellRate = 1m,
                TruevoSellRate = 1m,
                MastercardMidRate = 1m,
                ValidityDate = DateTime.UtcNow
            },
            new ExchangeRate
            {
                CurrencyNumber = 124,
                CurrencyCode = "USD",
                CurrencyExponent = 2,
                MastercardBuyRate = 11.56m,
                TruevoBuyRate = 11.75m,
                MastercardSellRate = 11.41m,
                TruevoSellRate = 11.25m,
                MastercardMidRate = 1.61m,
                ValidityDate = DateTime.UtcNow
            }
        };
        foreach (var rate in ratesToInsert)
        {
            await _exchangeRateRepository.InsertAsync(rate);
        }

        var input = new ConvertRateRequestDto
        {
            AmountToConvert = 10,
            SourceCurrencyCode = "USD",
            TargetCurrencyCode = "EUR",
            RateType = RateType.BUY,
            BaseMargin = 5,
            TargetMargin = 15,
            InverseMargin = true
        };
        var method = _exchangeRateService.GetType().GetMethod("GetExchangeRateByCurrencyCodeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        var task = (Task)method.Invoke(_exchangeRateService, new object[] { "USD" });
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        var usdRate = (ExchangeRate)resultProperty.GetValue(task);
        var expectedConvertedAmount = input.AmountToConvert / usdRate.TruevoBuyRate;
        expectedConvertedAmount *= 1 - input.BaseMargin / 100;
        expectedConvertedAmount = decimal.Round(expectedConvertedAmount, _exchangeRatesSettingsOptions.AmountRoundDecimalPlaces, MidpointRounding.AwayFromZero);

        // Act
        var result = await _exchangeRateService.ConvertRateAsync(input);

        // Assert
        Assert.Equal(input.AmountToConvert, result.AmountToConvert);
        Assert.Equal(input.SourceCurrencyCode, result.SourceCurrencyCode);
        Assert.Equal(input.TargetCurrencyCode, result.TargetCurrencyCode);
        Assert.Equal(input.RateType, result.RateType);
        Assert.Equal(expectedConvertedAmount, result.ConvertedAmount);
    }

    [Fact]
    public async Task ConvertRateAsync_Should_Calculate_Correct_Amout_From_Base_To_Target_Currency()
    {
        // Arrange
        var ratesToInsert = new List<ExchangeRate>
        {
            new ExchangeRate
            {
                CurrencyNumber = 123,
                CurrencyCode = "EUR",
                CurrencyExponent = 2,
                MastercardBuyRate = 1m,
                TruevoBuyRate = 1m,
                MastercardSellRate = 1m,
                TruevoSellRate = 1m,
                MastercardMidRate = 1m,
                ValidityDate = DateTime.UtcNow
            },
            new ExchangeRate
            {
                CurrencyNumber = 124,
                CurrencyCode = "USD",
                CurrencyExponent = 2,
                MastercardBuyRate = 11.56m,
                TruevoBuyRate = 11.75m,
                MastercardSellRate = 11.41m,
                TruevoSellRate = 11.25m,
                MastercardMidRate = 1.61m,
                ValidityDate = DateTime.UtcNow
            }
        };
        foreach (var rate in ratesToInsert)
        {
            await _exchangeRateRepository.InsertAsync(rate);
        }

        var input = new ConvertRateRequestDto
        {
            AmountToConvert = 10,
            SourceCurrencyCode = "EUR",
            TargetCurrencyCode = "USD",
            RateType = RateType.BUY,
            BaseMargin = 5,
            TargetMargin = 15
        };
        var method = _exchangeRateService.GetType().GetMethod("GetExchangeRateByCurrencyCodeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        var task = (Task)method.Invoke(_exchangeRateService, new object[] { "USD" });
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        var usdRate = (ExchangeRate)resultProperty.GetValue(task);
        var expectedConvertedAmount = input.AmountToConvert * usdRate.TruevoBuyRate;
        expectedConvertedAmount *= 1 + input.TargetMargin / 100;
        expectedConvertedAmount = decimal.Round(expectedConvertedAmount, _exchangeRatesSettingsOptions.AmountRoundDecimalPlaces, MidpointRounding.AwayFromZero);

        // Act
        var result = await _exchangeRateService.ConvertRateAsync(input);

        // Assert
        Assert.Equal(input.AmountToConvert, result.AmountToConvert);
        Assert.Equal(input.SourceCurrencyCode, result.SourceCurrencyCode);
        Assert.Equal(input.TargetCurrencyCode, result.TargetCurrencyCode);
        Assert.Equal(input.RateType, result.RateType);
        Assert.Equal(expectedConvertedAmount, result.ConvertedAmount);
    }

    [Fact]
    public async Task ConvertRateAsync_Should_Calculate_Correct_Amout_From_Base_To_Target_Currency_With_Inverse_Margin()
    {
        // Arrange
        var ratesToInsert = new List<ExchangeRate>
        {
            new ExchangeRate
            {
                CurrencyNumber = 123,
                CurrencyCode = "EUR",
                CurrencyExponent = 2,
                MastercardBuyRate = 1m,
                TruevoBuyRate = 1m,
                MastercardSellRate = 1m,
                TruevoSellRate = 1m,
                MastercardMidRate = 1m,
                ValidityDate = DateTime.UtcNow
            },
            new ExchangeRate
            {
                CurrencyNumber = 124,
                CurrencyCode = "USD",
                CurrencyExponent = 2,
                MastercardBuyRate = 11.56m,
                TruevoBuyRate = 11.75m,
                MastercardSellRate = 11.41m,
                TruevoSellRate = 11.25m,
                MastercardMidRate = 1.61m,
                ValidityDate = DateTime.UtcNow
            }
        };
        foreach (var rate in ratesToInsert)
        {
            await _exchangeRateRepository.InsertAsync(rate);
        }

        var input = new ConvertRateRequestDto
        {
            AmountToConvert = 10,
            SourceCurrencyCode = "EUR",
            TargetCurrencyCode = "USD",
            RateType = RateType.BUY,
            BaseMargin = 5,
            TargetMargin = 15,
            InverseMargin = true
        };
        var method = _exchangeRateService.GetType().GetMethod("GetExchangeRateByCurrencyCodeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        var task = (Task)method.Invoke(_exchangeRateService, new object[] { "USD" });
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        var usdRate = (ExchangeRate)resultProperty.GetValue(task);
        var expectedConvertedAmount = input.AmountToConvert * usdRate.TruevoBuyRate;
        expectedConvertedAmount *= 1 - input.TargetMargin / 100;
        expectedConvertedAmount = decimal.Round(expectedConvertedAmount, _exchangeRatesSettingsOptions.AmountRoundDecimalPlaces, MidpointRounding.AwayFromZero);

        // Act
        var result = await _exchangeRateService.ConvertRateAsync(input);

        // Assert
        Assert.Equal(input.AmountToConvert, result.AmountToConvert);
        Assert.Equal(input.SourceCurrencyCode, result.SourceCurrencyCode);
        Assert.Equal(input.TargetCurrencyCode, result.TargetCurrencyCode);
        Assert.Equal(input.RateType, result.RateType);
        Assert.Equal(expectedConvertedAmount, result.ConvertedAmount);
    }

    [Fact]
    public async Task ConvertRateAsync_Should_Calculate_Correct_Amout_From_Source_To_Target_Currency()
    {
        // Arrange
        var ratesToInsert = new List<ExchangeRate>
        {
            new ExchangeRate
            {
                CurrencyNumber = 123,
                CurrencyCode = "GBP",
                CurrencyExponent = 2,
                MastercardBuyRate = 1m,
                TruevoBuyRate = 1m,
                MastercardSellRate = 1m,
                TruevoSellRate = 1m,
                MastercardMidRate = 1m,
                ValidityDate = DateTime.UtcNow
            },
            new ExchangeRate
            {
                CurrencyNumber = 124,
                CurrencyCode = "USD",
                CurrencyExponent = 2,
                MastercardBuyRate = 11.56m,
                TruevoBuyRate = 11.75m,
                MastercardSellRate = 11.41m,
                TruevoSellRate = 11.25m,
                MastercardMidRate = 1.61m,
                ValidityDate = DateTime.UtcNow
            }
        };
        foreach (var rate in ratesToInsert)
        {
            await _exchangeRateRepository.InsertAsync(rate);
        }

        var input = new ConvertRateRequestDto
        {
            AmountToConvert = 10,
            SourceCurrencyCode = "GBP",
            TargetCurrencyCode = "USD",
            RateType = RateType.BUY,
            BaseMargin = 5,
            TargetMargin = 15
        };
        var method = _exchangeRateService.GetType().GetMethod("GetExchangeRateByCurrencyCodeAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        
        var task = (Task)method.Invoke(_exchangeRateService, new object[] { "USD" });
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        var usdRate = (ExchangeRate)resultProperty.GetValue(task);

        task = (Task)method.Invoke(_exchangeRateService, new object[] { "GBP" });
        await task.ConfigureAwait(false);
        resultProperty = task.GetType().GetProperty("Result");
        var gbpRate = (ExchangeRate)resultProperty.GetValue(task);

        var expectedConvertRate = (1 / gbpRate.TruevoBuyRate) * usdRate.TruevoBuyRate;
        expectedConvertRate *= 1 + input.BaseMargin / 100;
        expectedConvertRate *= 1 + input.TargetMargin / 100;
        var expectedConvertedAmount = decimal.Round(input.AmountToConvert * expectedConvertRate, _exchangeRatesSettingsOptions.AmountRoundDecimalPlaces, MidpointRounding.AwayFromZero);

        // Act
        var result = await _exchangeRateService.ConvertRateAsync(input);

        // Assert
        Assert.Equal(input.AmountToConvert, result.AmountToConvert);
        Assert.Equal(input.SourceCurrencyCode, result.SourceCurrencyCode);
        Assert.Equal(input.TargetCurrencyCode, result.TargetCurrencyCode);
        Assert.Equal(input.RateType, result.RateType);
        Assert.Equal(expectedConvertedAmount, result.ConvertedAmount);
    }

    [Fact]
    public async Task ConvertRateAsync_Should_Calculate_Correct_Amout_From_Source_To_Target_Currency_With_Inverse_Margin()
    {
        // Arrange
        var ratesToInsert = new List<ExchangeRate>
        {
            new ExchangeRate
            {
                CurrencyNumber = 123,
                CurrencyCode = "GBP",
                CurrencyExponent = 2,
                MastercardBuyRate = 1m,
                TruevoBuyRate = 1m,
                MastercardSellRate = 1m,
                TruevoSellRate = 1m,
                MastercardMidRate = 1m,
                ValidityDate = DateTime.UtcNow
            },
            new ExchangeRate
            {
                CurrencyNumber = 124,
                CurrencyCode = "USD",
                CurrencyExponent = 2,
                MastercardBuyRate = 11.56m,
                TruevoBuyRate = 11.75m,
                MastercardSellRate = 11.41m,
                TruevoSellRate = 11.25m,
                MastercardMidRate = 1.61m,
                ValidityDate = DateTime.UtcNow
            }
        };
        foreach (var rate in ratesToInsert)
        {
            await _exchangeRateRepository.InsertAsync(rate);
        }

        var input = new ConvertRateRequestDto
        {
            AmountToConvert = 10,
            SourceCurrencyCode = "GBP",
            TargetCurrencyCode = "USD",
            RateType = RateType.BUY,
            BaseMargin = 5,
            TargetMargin = 15,
            InverseMargin = true
        };
        var method = _exchangeRateService.GetType().GetMethod("GetExchangeRateByCurrencyCodeAsync", BindingFlags.Instance | BindingFlags.NonPublic);

        var task = (Task)method.Invoke(_exchangeRateService, new object[] { "USD" });
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        var usdRate = (ExchangeRate)resultProperty.GetValue(task);

        task = (Task)method.Invoke(_exchangeRateService, new object[] { "GBP" });
        await task.ConfigureAwait(false);
        resultProperty = task.GetType().GetProperty("Result");
        var gbpRate = (ExchangeRate)resultProperty.GetValue(task);

        var expectedConvertRate = (1 / gbpRate.TruevoBuyRate) * usdRate.TruevoBuyRate;
        expectedConvertRate *= 1 - input.BaseMargin / 100;
        expectedConvertRate *= 1 - input.TargetMargin / 100;
        var expectedConvertedAmount = decimal.Round(input.AmountToConvert * expectedConvertRate, _exchangeRatesSettingsOptions.AmountRoundDecimalPlaces, MidpointRounding.AwayFromZero);

        // Act
        var result = await _exchangeRateService.ConvertRateAsync(input);

        // Assert
        Assert.Equal(input.AmountToConvert, result.AmountToConvert);
        Assert.Equal(input.SourceCurrencyCode, result.SourceCurrencyCode);
        Assert.Equal(input.TargetCurrencyCode, result.TargetCurrencyCode);
        Assert.Equal(input.RateType, result.RateType);
        Assert.Equal(expectedConvertedAmount, result.ConvertedAmount);
    }
}