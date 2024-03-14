using MediatR;
using OrderAgregator.API.Cache;
using OrderAgregator.API.Handlers.Commands;
using OrderAgregator.API.Models;
using OrderAgregator.API.Services;

namespace OrderAgregator.API.Handlers
{
    /// <summary>
    ///     Save order & signal background service for sending to external service
    /// </summary>
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<Unit, DomainError>>
    {
        private readonly IOrderCache _orderCache;
        private readonly ILimitedOrderBackgroundSeviceSignaller _serviceSignaller;

        public CreateOrderHandler(IOrderCache orderCache, ILimitedOrderBackgroundSeviceSignaller serviceThrottler)
        {
            _orderCache = orderCache;
            _serviceSignaller = serviceThrottler;
        }

        public async Task<Result<Unit, DomainError>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            await _orderCache.SaveOrders(request.Orders);

            _serviceSignaller.Signal();

            return Unit.Value;
        }
    }
}
