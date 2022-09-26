namespace TruevoExchangeRateAPI.Data
{
    public static class CurrencyCodeNumberConverter
    {
        private static Dictionary<string, int> _currencyCodeToNumberDictionary;
        private static Dictionary<int, string> _currencyNumberToCodeDictionary;

        static CurrencyCodeNumberConverter()
        {
            _currencyCodeToNumberDictionary = new Dictionary<string, int>
            {
                { "AFN", 971 },
                { "ALL", 008 },
                { "DZD", 012 },
                { "USD", 840 },
                { "EUR", 978 },
                { "AOA", 973 },
                { "XCD", 951 },
                { "ARS", 032 },
                { "AMD", 051 },
                { "AWG", 533 },
                { "AUD", 036 },
                { "AZN", 944 },
                { "BSD", 044 },
                { "BHD", 048 },
                { "BDT", 050 },
                { "BBD", 052 },
                { "BYN", 933 },
                { "BZD", 084 },
                { "XOF", 952 },
                { "BMD", 060 },
                { "BTN", 064 },
                { "INR", 356 },
                { "BOB", 068 },
                { "BOV", 984 },
                { "BAM", 977 },
                { "BWP", 072 },
                { "NOK", 578 },
                { "BRL", 986 },
                { "BND", 096 },
                { "BGN", 975 },
                { "BIF", 108 },
                { "CVE", 132 },
                { "KHR", 116 },
                { "XAF", 950 },
                { "CAD", 124 },
                { "KYD", 136 },
                { "CLF", 990 },
                { "CLP", 152 },
                { "CNY", 156 },
                { "COP", 170 },
                { "COU", 970 },
                { "KMF", 174 },
                { "CDF", 976 },
                { "NZD", 554 },
                { "CRC", 188 },
                { "HRK", 191 },
                { "CUC", 931 },
                { "CUP", 192 },
                { "ANG", 532 },
                { "CZK", 203 },
                { "DKK", 208 },
                { "DJF", 262 },
                { "DOP", 214 },
                { "EGP", 818 },
                { "SVC", 222 },
                { "ERN", 232 },
                { "ETB", 230 },
                { "FKP", 238 },
                { "FJD", 242 },
                { "XPF", 953 },
                { "GMD", 270 },
                { "GEL", 981 },
                { "GHS", 936 },
                { "GIP", 292 },
                { "GTQ", 320 },
                { "GBP", 826 },
                { "GNF", 324 },
                { "GYD", 328 },
                { "HTG", 332 },
                { "HNL", 340 },
                { "HKD", 344 },
                { "HUF", 348 },
                { "ISK", 352 },
                { "IDR", 360 },
                { "XDR", 960 },
                { "IRR", 364 },
                { "IQD", 368 },
                { "ILS", 376 },
                { "JMD", 388 },
                { "JPY", 392 },
                { "JOD", 400 },
                { "KZT", 398 },
                { "KES", 404 },
                { "KPW", 408 },
                { "KRW", 410 },
                { "KWD", 414 },
                { "KGS", 417 },
                { "LAK", 418 },
                { "LBP", 422 },
                { "LSL", 426 },
                { "ZAR", 710 },
                { "LRD", 430 },
                { "LYD", 434 },
                { "CHF", 756 },
                { "MOP", 446 },
                { "MGA", 969 },
                { "MWK", 454 },
                { "MYR", 458 },
                { "MVR", 462 },
                { "MRU", 929 },
                { "MUR", 480 },
                { "XUA", 965 },
                { "MXN", 484 },
                { "MXV", 979 },
                { "MDL", 498 },
                { "MNT", 496 },
                { "MAD", 504 },
                { "MZN", 943 },
                { "MMK", 104 },
                { "NAD", 516 },
                { "NPR", 524 },
                { "NIO", 558 },
                { "NGN", 566 },
                { "OMR", 512 },
                { "PKR", 586 },
                { "PAB", 590 },
                { "PGK", 598 },
                { "PYG", 600 },
                { "PEN", 604 },
                { "PHP", 608 },
                { "PLN", 985 },
                { "QAR", 634 },
                { "MKD", 807 },
                { "RON", 946 },
                { "RUB", 643 },
                { "RWF", 646 },
                { "SHP", 654 },
                { "WST", 882 },
                { "STN", 930 },
                { "SAR", 682 },
                { "RSD", 941 },
                { "SCR", 690 },
                { "SLE", 925 },
                { "SGD", 702 },
                { "XSU", 994 },
                { "SBD", 090 },
                { "SOS", 706 },
                { "SSP", 728 },
                { "LKR", 144 },
                { "SDG", 938 },
                { "SRD", 968 },
                { "SZL", 748 },
                { "SEK", 752 },
                { "CHE", 947 },
                { "CHW", 948 },
                { "SYP", 760 },
                { "TWD", 901 },
                { "TJS", 972 },
                { "TZS", 834 },
                { "THB", 764 },
                { "TOP", 776 },
                { "TTD", 780 },
                { "TND", 788 },
                { "TRY", 949 },
                { "TMT", 934 },
                { "UGX", 800 },
                { "UAH", 980 },
                { "AED", 784 },
                { "USN", 997 },
                { "UYI", 940 },
                { "UYU", 858 },
                { "UZS", 860 },
                { "VUV", 548 },
                { "VEF", 937 },
                { "VED", 926 },
                { "VND", 704 },
                { "YER", 886 },
                { "ZMW", 967 },
                { "ZWL", 932 },
                { "MRO", 478 },
                { "STD", 678 },
                { "SLL", 694 },
                { "BYR", 974 }
            };

            _currencyNumberToCodeDictionary = _currencyCodeToNumberDictionary.ToDictionary(x => x.Value, x => x.Key);
        }

        /// <summary>
        /// Method retrieves currency number based on currency code.
        /// If the currency code is not present in the internal dictionary,
        /// method throws an exception.
        /// </summary>
        /// <param name="currencyCode">Currency code.</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static int GetCurrencyNumber(string currencyCode)
        {
            // intentionally not checking if the key exists. If there is an error, dictionary should be updated
            return _currencyCodeToNumberDictionary[currencyCode];
        }

        /// <summary>
        /// Method retrieves currency code based on currency number.
        /// If the currency number is not present in the internal dictionary,
        /// method throws an exception.
        /// </summary>
        /// <param name="currencyNumber">Currency number.</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static string GetCurrencyCode(int currencyNumber)
        {
            // intentionally not checking if the key exists. If there is an error, dictionary should be updated
            return _currencyNumberToCodeDictionary[currencyNumber];
        }

        /// <summary>
        /// Method detects if the provided currency is valid currency.
        /// Currency can be provided as code or number.
        /// </summary>
        /// <param name="currency">String representing currency code or currency number.</param>
        /// <returns></returns>
        public static bool IsValidCurrency(string currency)
        {
            if (_currencyCodeToNumberDictionary.ContainsKey(currency))
            {
                return true;
            }
            if (!int.TryParse(currency, out int result))
            {
                return false;
            }
            if (_currencyNumberToCodeDictionary.ContainsKey(result))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method returns the currency code.
        /// In the case of invalid currency, method throws an exception.
        /// </summary>
        /// <param name="currency">String representing currency code or currency number.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GetCurrencyCode(string currency)
        {
            if (!IsValidCurrency(currency))
            {
                throw new ArgumentException($"Provided currency: {currency} is not the valid one.");
            }
            if (_currencyCodeToNumberDictionary.ContainsKey(currency))
            {
                return currency;
            }

            return _currencyNumberToCodeDictionary[int.Parse(currency)];
        }
    }
}
