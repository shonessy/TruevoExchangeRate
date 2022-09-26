using System.Text.Json.Serialization;

namespace TruevoExchangeRateAPI.Data.DTOs
{
    public class TrailerRecord
    {
        [JsonPropertyName("total_records")]
        public int TotalRecords { get; set; }

        [JsonPropertyName("header")]
        public int hash_total { get; set; }
    }
}
