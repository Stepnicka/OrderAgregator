using FluentValidation;
using OrderAgregator.API.Handlers.Commands;
using OrderAgregator.API.Models;

namespace OrderAgregator.API.Validation.Validators
{
    public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderValidator(IValidator<Order> orderValidator)
        {
            this.RuleFor(x => x.Orders)
                .NotEmpty()
                .WithMessage("No orders provided.");

            this.RuleForEach(x => x.Orders)
                .SetValidator(orderValidator);
        }
    }
}
