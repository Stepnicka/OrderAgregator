using NSubstitute;
using OrderAgregator.API.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrderAgregator.Test.Integration
{
    public class OrderEndpointTest : TestBase
    {
        [Fact]
        public async Task Create_ShouldFail_Invalid_Quantity()
        {
            //Arrange
            OrderDto[] request = [new OrderDto { ProductId = "22", Quantity = 0 }];

            //act
            var response = await _client.PostAsJsonAsync("api/order/create", request, CancellationToken.None);

            //assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_ShouldFail_Invalid_ProductId()
        {
            //Arrange
            OrderDto[] request = [new OrderDto { ProductId = "", Quantity = 0 }];

            //act
            var response = await _client.PostAsJsonAsync("api/order/create", request, CancellationToken.None);

            //assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_ShouldFail_Invalid_NoOrder()
        {
            //Arrange
            OrderDto[] request = [];

            //act
            var response = await _client.PostAsJsonAsync("api/order/create", request, CancellationToken.None);

            //assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_ShouldPass()
        {
            //Arrange
            OrderDto[] request = [
                new OrderDto { ProductId = "22", Quantity = 4 },
                new OrderDto { ProductId = "22", Quantity = 5 },
                new OrderDto { ProductId = "11", Quantity = 1 },
                new OrderDto { ProductId = "12", Quantity = 1 },
                new OrderDto { ProductId = "13", Quantity = 1 },
            ];

            ImmutableArray<API.Services.ExternalApiServices.Models.Order> expected = [
                new API.Services.ExternalApiServices.Models.Order { ProductId = "22", Quantity = 9 },
                new API.Services.ExternalApiServices.Models.Order { ProductId = "11", Quantity = 1 },
                new API.Services.ExternalApiServices.Models.Order { ProductId = "12", Quantity = 1 },
                new API.Services.ExternalApiServices.Models.Order { ProductId = "13", Quantity = 1 },
            ];

            _externalApi.SendOrders(default).ReturnsForAnyArgs(Task.CompletedTask);

            RateLimiterConfiguration limiterConfiguration;

            using (var scope = _scopeFactory.CreateScope())
                limiterConfiguration = scope.ServiceProvider.GetRequiredService<IOptions<RateLimiterConfiguration>>().Value;

            //act
            var response = await _client.PostAsJsonAsync("api/order/create", request, CancellationToken.None);

            //assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var calls = _externalApi.ReceivedCalls();
            Assert.Empty(calls);

            /* Orders are stored in a persistent storage and sending to external service is rateLimited
             * We need to wait for rate limiter to allow passing of orders to service and then check if service recieved them 
             */
            await Task.Delay(TimeSpan.FromSeconds(limiterConfiguration.Seconds));

            await _externalApi.Received(1).SendOrders(Arg.Any<ImmutableArray<API.Services.ExternalApiServices.Models.Order>>());

            var argument = _externalApi.ReceivedCalls().Single().GetArguments().Single()!;

            var passedArray = (ImmutableArray<API.Services.ExternalApiServices.Models.Order>)argument;

            Assert.True(passedArray.SequenceEqual(expected));
        }

        [Fact]
        public async Task Create_ShouldPass_SpacedBySeconds()
        {
            //Arrange
            OrderDto[] request = [
                new OrderDto { ProductId = "22", Quantity = 4 },
                new OrderDto { ProductId = "22", Quantity = 5 },
                new OrderDto { ProductId = "11", Quantity = 1 },
                new OrderDto { ProductId = "12", Quantity = 1 },
                new OrderDto { ProductId = "13", Quantity = 1 },
            ];

            RateLimiterConfiguration limiterConfiguration;

            using (var scope = _scopeFactory.CreateScope())
                limiterConfiguration = scope.ServiceProvider.GetRequiredService<IOptions<RateLimiterConfiguration>>().Value;

            var externalServiceCallTimes = new List<DateTime>();
            _externalApi.SendOrders(default).ReturnsForAnyArgs(x => { externalServiceCallTimes.Add(DateTime.Now); return Task.CompletedTask; });

            //act
            for (int i = 0; i < limiterConfiguration.Seconds * 3; i++)
            {
                await _client.PostAsJsonAsync("api/order/create", request, CancellationToken.None);
                await Task.Delay(TimeSpan.FromSeconds(limiterConfiguration.Seconds / 2));
            }

            //Assert
            Assert.NotEmpty(_externalApi.ReceivedCalls());

            Assert.NotEmpty(externalServiceCallTimes);
            Assert.True(externalServiceCallTimes.Count > 1);

            for (int i = 1; i < externalServiceCallTimes.Count; i++)
            {
                var prev = externalServiceCallTimes[i - 1];
                var cur = externalServiceCallTimes[i];
                var secondsBetween = Math.Round((cur - prev).TotalSeconds);
                Assert.True(secondsBetween >= limiterConfiguration.Seconds);
            }
        }
    }
}
