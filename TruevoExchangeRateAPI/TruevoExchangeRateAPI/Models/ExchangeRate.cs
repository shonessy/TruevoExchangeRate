using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TruevoExchangeRateAPI.Models
{
    [Index(nameof(CurrencyNumber), IsUnique = true)]
    [Index(nameof(CurrencyCode), IsUnique = true)]
    public class ExchangeRate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CurrencyNumber { get; set; }

        [Required]
        public string CurrencyCode { get; set; }
        
        [Required]
        public int CurrencyExponent { get; set; }
        
        [Required]
        public decimal BuyRate { get; set; }
        
        [Required]
        public decimal MidRate { get; set; }
        
        [Required]
        public decimal SellRate { get; set; }

        [Required]
        public DateTime ValidityDate { get; set; }
    }
}
