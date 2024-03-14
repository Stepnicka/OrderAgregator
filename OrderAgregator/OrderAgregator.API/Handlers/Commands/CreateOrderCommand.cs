using MediatR;
using OrderAgregator.API.Models;

namespace OrderAgregator.API.Handlers.Commands
{
    /// <remarks> Is handled by <see cref="CreateOrderHandler"/> </remarks>
    public record CreateOrderCommand : IRequest<Result<Unit, DomainError>>
    {
        public required IReadOnlyList<Order> Orders { get; init; }
    }
}
