using TruevoExchangeRateAPI.Data.DTOs;

namespace TruevoExchangeRateAPI.Tests
{
    public class RateType_Test
    {
        [Fact]
        public void IsValidRateType_Should_Return_True_On_Buy_Rate_Type()
        {
            // Arrange

            // Act
            var result1 = RateType.IsValidRateType("BUY");
            var result2 = RateType.IsValidRateType("buy");

            // Assert
            Assert.True(result1);
            Assert.True(result2);
        }

        [Fact]
        public void IsValidRateType_Should_Return_True_On_Sell_Rate_Type()
        {
            // Arrange

            // Act
            var result1 = RateType.IsValidRateType("SELL");
            var result2 = RateType.IsValidRateType("sell");

            // Assert
            Assert.True(result1);
            Assert.True(result2);
        }

        [Fact]
        public void IsValidRateType_Should_Return_True_On_Invalid_Rate_Type()
        {
            // Arrange

            // Act
            var result1 = RateType.IsValidRateType("test");

            // Assert
            Assert.False(result1);
        }
    }
}
