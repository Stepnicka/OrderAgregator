using Bogus;
using NSubstitute;
using System.Collections.Immutable;
using Xunit;

namespace OrderAgregator.Test.Unit.Handlers
{
    public class TST_SendAggregatedOrdersHandler
    {
        private readonly Faker<API.Models.Order> _orderGenerator =
            new Faker<API.Models.Order>()
                .StrictMode(true)
                .RuleFor(property: order => order.ProductId, f => f.Random.Number(1, 999).ToString())
                .RuleFor(property: order => order.Quantity, f => f.Random.Number(1, 99))
                .UseSeed(999);

        [Fact]
        public async Task Handle_Cache_Returns_No_Orders()
        {
            // Arrange
            var cache = Substitute.For<API.Cache.IOrderCache>();
            var externalApi = Substitute.For<API.Services.ExternalApiServices.IExternalApi>();
            var handler = new API.Handlers.SendAggregatedOrdersHandler(cache, externalApi);

            cache.GetOrders().Returns(new List<API.Models.Order>());

            // Act
            var result = await handler.Handle(new API.Handlers.Commands.SendAggregatedOrdersCommand(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            await externalApi.DidNotReceive().SendOrders(Arg.Any<ImmutableArray<API.Services.ExternalApiServices.Models.Order>>());
        }

        [Fact]
        public async Task Handle_Cache_Returns_Orders_Passing_To_Service_Ok()
        {
            // Arrange
            var cache = Substitute.For<API.Cache.IOrderCache>();
            var externalApi = Substitute.For<API.Services.ExternalApiServices.IExternalApi>();
            var handler = new API.Handlers.SendAggregatedOrdersHandler(cache, externalApi);

            var orders = _orderGenerator.GenerateBetween(1, 10);
            cache.GetOrders().Returns(orders);

            var expected = orders.GroupBy(x => x.ProductId)
                .Select(g => new API.Services.ExternalApiServices.Models.Order { ProductId = g.Key, Quantity = g.Sum(o => o.Quantity) })
                .ToImmutableArray();

            // Act
            var result = await handler.Handle(new API.Handlers.Commands.SendAggregatedOrdersCommand(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            await externalApi.Received(1).SendOrders(Arg.Any<ImmutableArray<API.Services.ExternalApiServices.Models.Order>>());

            var argument = externalApi.ReceivedCalls().Single().GetArguments().Single()!;
            var passedArray = (ImmutableArray<API.Services.ExternalApiServices.Models.Order>)argument;

            Assert.True(passedArray.SequenceEqual(expected));
        }

        [Fact]
        public async Task Handle_Cache_Returns_Orders_Passing_To_Service_Fail()
        {
            // Arrange
            var cache = Substitute.For<API.Cache.IOrderCache>();
            var externalApi = Substitute.For<API.Services.ExternalApiServices.IExternalApi>();
            var handler = new API.Handlers.SendAggregatedOrdersHandler(cache, externalApi);

            var orders = _orderGenerator.GenerateBetween(1, 10);
            cache.GetOrders().Returns(orders);
            externalApi.SendOrders(default).ReturnsForAnyArgs(x => { throw new Exception(); });

            // Act
            await Assert.ThrowsAsync<Exception>(() => handler.Handle(new API.Handlers.Commands.SendAggregatedOrdersCommand(), CancellationToken.None));

            /* Check that orders were pushed back to cache on exception from external service */
            await cache.Received(1).SaveOrders(orders);
        }
    }
}
