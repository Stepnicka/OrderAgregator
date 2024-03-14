using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderAgregator.API.Models;
using System.Collections.Immutable;

namespace OrderAgregator.API.Controllers
{
    [Route("api/[controller]")]
    [Controller]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost(nameof(Create))]
        public async Task<IActionResult> Create([FromBody] ImmutableArray<OrderDto> orders)
        {
            var result = await _mediator.Send(new Handlers.Commands.CreateOrderCommand() 
            {
                Orders = orders.MapToOrders().ToList()
            });

            return result.Match(
                success: _ => Ok(),
                failure: error => error.Match<IActionResult>(
                    validationError => BadRequest(validationError),
                    InternalError => StatusCode(StatusCodes.Status500InternalServerError)
            ));
        }
    }
}
