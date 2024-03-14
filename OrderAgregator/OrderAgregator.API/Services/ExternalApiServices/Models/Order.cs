using System.Text.Json.Serialization;

namespace OrderAgregator.API.Services.ExternalApiServices.Models
{
    public record Order
    {
        [JsonPropertyName("productId")]
        public required string ProductId { get; init; }

        [JsonPropertyName("quantity")]
        public required int Quantity { get; init; }
    }
}
