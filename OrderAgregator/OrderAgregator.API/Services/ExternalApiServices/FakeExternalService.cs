using OrderAgregator.API.Services.ExternalApiServices.Models;
using System.Collections.Immutable;
using System.Text.Json;

namespace OrderAgregator.API.Services.ExternalApiServices
{
    public class FakeExternalService : IExternalApi
    {
        private static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

        public async Task SendOrders(ImmutableArray<Order> orders)
        {
            var json = JsonSerializer.Serialize(orders, jsonOptions);

            await Console.Out.WriteLineAsync(DateTime.Now.ToString("hh:mm:ss"));
            await Console.Out.WriteLineAsync(json);
        }
    }
}
