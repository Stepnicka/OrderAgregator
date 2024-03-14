using Bogus;
using NSubstitute;
using Xunit;

namespace OrderAgregator.Test.Unit.Handlers
{
    public class TST_CreateOrderHandler
    {
        private readonly Faker<API.Models.Order> _orderGenerator =
            new Faker<API.Models.Order>()
                .StrictMode(true)
                .RuleFor(property: order => order.ProductId, f => f.Random.Number(1, 999).ToString())
                .RuleFor(property: order => order.Quantity, f => f.Random.Number(1, 99))
                .UseSeed(999);

        [Fact]
        public async Task Handle_Should_Pass_Orders_To_Cache()
        {
            // Arrange
            var cache = Substitute.For<API.Cache.IOrderCache>();
            var signaller = Substitute.For<API.Services.ILimitedOrderBackgroundSeviceSignaller>();
            var handler = new API.Handlers.CreateOrderHandler(cache, signaller);

            var command = new API.Handlers.Commands.CreateOrderCommand()
            {
                Orders = _orderGenerator.GenerateBetween(1, 99)
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            await cache.Received(1).SaveOrders(command.Orders);

            signaller.Received(1).Signal();
        }
    }
}
