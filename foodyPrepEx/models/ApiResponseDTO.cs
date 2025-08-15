using System.Text.Json.Serialization;

namespace Foody.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("foodyId")]
        public string? FoodyId { get; set; }
    }
}
