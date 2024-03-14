using FluentValidation;
using MediatR;
using OrderAgregator.API.Models;

namespace OrderAgregator.API.Validation
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, Result<TResponse, DomainError>> where TRequest : notnull
    {
        private readonly IValidator<TRequest> _validator;

        public ValidationBehavior(IValidator<TRequest> validator)
        {
            this._validator = validator;
        }

        public async Task<Result<TResponse, DomainError>> Handle(TRequest request, RequestHandlerDelegate<Result<TResponse, DomainError>> next, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, CancellationToken.None);

            if(validationResult.IsValid == false)
            {
                return (DomainError) new ValidationError() { Errors = validationResult.Errors };
            }

            return await next();
        }
    }
}
