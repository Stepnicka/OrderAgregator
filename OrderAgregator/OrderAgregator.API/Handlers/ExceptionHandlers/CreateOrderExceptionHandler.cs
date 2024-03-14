using MediatR;
using MediatR.Pipeline;
using OrderAgregator.API.Handlers.Commands;
using OrderAgregator.API.Models;

namespace OrderAgregator.API.Handlers.ExceptionHandlers
{
    public class CreateOrderExceptionHandler : IRequestExceptionHandler<CreateOrderCommand, Result<Unit, DomainError>, Exception>
    {
        private readonly ILogger<CreateOrderExceptionHandler> _logger;

        public CreateOrderExceptionHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CreateOrderExceptionHandler>();    
        }

        public Task Handle(CreateOrderCommand request, Exception exception, RequestExceptionHandlerState<Result<Unit, DomainError>> state, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Ups... something happened");

            state.SetHandled((DomainError) new InternalError());
            
            return Task.CompletedTask;
        }
    }
}
