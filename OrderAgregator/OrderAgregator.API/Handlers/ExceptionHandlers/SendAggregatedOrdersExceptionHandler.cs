using MediatR;
using MediatR.Pipeline;
using OrderAgregator.API.Handlers.Commands;
using OrderAgregator.API.Models;

namespace OrderAgregator.API.Handlers.ExceptionHandlers
{
    public class SendAggregatedOrdersExceptionHandler : IRequestExceptionHandler<SendAggregatedOrdersCommand, Result<Unit, DomainError>, Exception>
    {
        private readonly ILogger<CreateOrderExceptionHandler> _logger;

        public SendAggregatedOrdersExceptionHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CreateOrderExceptionHandler>();
        }

        public Task Handle(SendAggregatedOrdersCommand request, Exception exception, RequestExceptionHandlerState<Result<Unit, DomainError>> state, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Ups... something happened");

            state.SetHandled((DomainError)new InternalError());

            return Task.CompletedTask;
        }
    }
}
