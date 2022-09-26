using TruevoExchangeRateAPI.Data;

namespace TruevoExchangeRateAPI.Tests
{
    public class CurrencyCodeNumberConverter_Test
    {

        [Fact]
        public void GetCurrencyNumber_Should_Throw_KeyNotFoundException_On_Invalid_Currency_Code()
        {
            // Arrange

            // Act

            // Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => CurrencyCodeNumberConverter.GetCurrencyNumber("TEST"));
        }

        [Fact]
        public void GetCurrencyNumber_Should_Return_Correct_Currency_Number()
        {
            // Arrange

            // Act
            var currencyNumber = CurrencyCodeNumberConverter.GetCurrencyNumber("USD");

            // Assert
            Assert.Equal(840, currencyNumber);
        }

        [Fact]
        public void GetCurrencyCode_Should_Throw_KeyNotFoundException_On_Invalid_Currency_Number()
        {
            // Arrange

            // Act

            // Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => CurrencyCodeNumberConverter.GetCurrencyCode(123456));
        }

        [Fact]
        public void GetCurrencyCode_Should_Return_Correct_Currency_Code()
        {
            // Arrange

            // Act
            var currencyNumber = CurrencyCodeNumberConverter.GetCurrencyCode(840);

            // Assert
            Assert.Equal("USD", currencyNumber);
        }

        [Fact]
        public void IsValidCurrency_Should_Return_False_On_Invalid_Currency_Number()
        {
            // Arrange

            // Act

            // Assert
            Assert.False(CurrencyCodeNumberConverter.IsValidCurrency("113313"));
        }

        [Fact]
        public void IsValidCurrency_Should_Return_False_On_Invalid_Currency_Format()
        {
            // Arrange

            // Act

            // Assert
            Assert.False(CurrencyCodeNumberConverter.IsValidCurrency("1133dafdas13"));
        }

        [Fact]
        public void IsValidCurrency_Should_Return_False_On_Invalid_Currency_Code()
        {
            // Arrange

            // Act

            // Assert
            Assert.False(CurrencyCodeNumberConverter.IsValidCurrency("TEST"));
        }

        [Fact]
        public void IsValidCurrency_Should_Return_True_On_Valid_Currency_Code()
        {
            // Arrange

            // Act

            // Assert
            Assert.True(CurrencyCodeNumberConverter.IsValidCurrency("USD"));
        }

        [Fact]
        public void IsValidCurrency_Should_Return_True_On_Valid_Currency_Number()
        {
            // Arrange

            // Act

            // Assert
            Assert.True(CurrencyCodeNumberConverter.IsValidCurrency("840"));
        }

        [Fact]
        public void GetCurrencyCode_Should_Return_Currency_Code_On_Correct_Currency_Code()
        {
            // Arrange

            // Act
            var result = CurrencyCodeNumberConverter.GetCurrencyCode("EUR");

            // Assert
            Assert.Equal("EUR", result);
        }

        [Fact]
        public void GetCurrencyCode_Should_Return_Currency_Code_On_Correct_Currency_Number()
        {
            // Arrange

            // Act
            var result = CurrencyCodeNumberConverter.GetCurrencyCode("840");

            // Assert
            Assert.Equal("USD", result);
        }

        [Fact]
        public void GetCurrencyCode_Should_Throw_Exception_Code_On_Wrong_Currency_Code()
        {
            // Arrange

            // Act

            // Assert
            var exception = Assert.Throws<ArgumentException>(() => CurrencyCodeNumberConverter.GetCurrencyCode("TEST"));
            Assert.Equal("Provided currency: TEST is not the valid one.", exception.Message);
        }

        [Fact]
        public void GetCurrencyCode_Should_Throw_Exception_Code_On_Wrong_Currency_Number()
        {
            // Arrange

            // Act

            // Assert
            var exception = Assert.Throws<ArgumentException>(() => CurrencyCodeNumberConverter.GetCurrencyCode("123456"));
            Assert.Equal("Provided currency: 123456 is not the valid one.", exception.Message);
        }
    }

}
