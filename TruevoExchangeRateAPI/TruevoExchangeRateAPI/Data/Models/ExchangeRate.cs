using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TruevoExchangeRateAPI.Data.Models
{
    [Index(nameof(CurrencyNumber), IsUnique = true)]
    [Index(nameof(CurrencyCode), IsUnique = true)]
    public class ExchangeRate : BaseEntity<int>
    {
        [Required]
        public int CurrencyNumber { get; set; }

        [Required]
        public string? CurrencyCode { get; set; }
        
        [Required]
        public int CurrencyExponent { get; set; }
        
        [Required]
        public decimal MastercardBuyRate { get; set; }
        
        [Required]
        public decimal TruevoBuyRate { get; set; }

        [Required]
        public decimal MastercardSellRate { get; set; }

        [Required]
        public decimal TruevoSellRate { get; set; }

        [Required]
        public decimal MastercardMidRate { get; set; }

        [Required]
        public decimal TruevoMidRate { get; set; }

        [Required]
        public DateTime ValidityDate { get; set; }
    }
}
