using System.Text.Json.Serialization;

namespace TruevoExchangeRateAPI.Data.DTOs
{
    public class HeaderRecord
    {
        [JsonPropertyName("date")]
        public string? Date { get; set; }
    }
}
