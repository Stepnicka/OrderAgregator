using MediatR;
using OrderAgregator.API.Cache;
using OrderAgregator.API.Handlers.Commands;
using OrderAgregator.API.Models;
using OrderAgregator.API.Services.ExternalApiServices;
using System.Collections.Immutable;

namespace OrderAgregator.API.Handlers
{
    /// <summary>
    ///     Fetch saved orders and pass them to an external service
    /// </summary>
    public class SendAggregatedOrdersHandler : IRequestHandler<SendAggregatedOrdersCommand, Result<Unit, DomainError>>
    {
        private readonly IOrderCache _orderCache;
        private readonly IExternalApi _externalApi;

        public SendAggregatedOrdersHandler(IOrderCache orderCache, IExternalApi externalApi)
        {
            _orderCache = orderCache;
            _externalApi = externalApi;
        }

        public async Task<Result<Unit, DomainError>> Handle(SendAggregatedOrdersCommand request, CancellationToken cancellationToken)
        {
            var orders = await _orderCache.GetOrders();

            if (orders.Count == 0)
                return Unit.Value;

            var aggregatedOrders = orders.GroupBy(x => x.ProductId)
                .Select(g => new Services.ExternalApiServices.Models.Order { ProductId = g.Key, Quantity = g.Sum(o => o.Quantity) })
                .ToImmutableArray();

            try
            {
                await _externalApi.SendOrders(aggregatedOrders);
            }
            catch /* In case of an error on external service, push back to cache */
            {
                await _orderCache.SaveOrders(orders);

                throw;
            }

            return Unit.Value;
        }
    }
}