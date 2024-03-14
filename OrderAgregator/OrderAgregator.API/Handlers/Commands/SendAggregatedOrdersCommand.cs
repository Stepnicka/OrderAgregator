using MediatR;
using OrderAgregator.API.Models;

namespace OrderAgregator.API.Handlers.Commands
{
    /// <remarks> Is handled by <see cref="SendAggregatedOrdersHandler"/> </remarks>
    public class SendAggregatedOrdersCommand : IRequest<Result<Unit, DomainError>>
    {
    }
}
