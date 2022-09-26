using System.Text.Json.Serialization;

namespace TruevoExchangeRateAPI.Data.DTOs
{
    public class ImportExchangeRatesDto
    {
        [JsonPropertyName("header")]
        public HeaderRecord? HeaderRecord { get; set; }

        [JsonPropertyName("detail_records")]
        public IEnumerable<DetailRecord>? DetailRecords { get; set; } = new List<DetailRecord>();

        [JsonPropertyName("trailer")]
        public TrailerRecord? TrailerRecord { get; set; }
    }
}
