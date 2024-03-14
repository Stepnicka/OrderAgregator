using FluentValidation;
using OrderAgregator.API.Models;

namespace OrderAgregator.API.Validation.Validators
{
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            this.RuleFor(x => x.Quantity)
                .GreaterThan(0);

            this.RuleFor(x => x.ProductId)
                .NotEmpty();
        }
    }
}
