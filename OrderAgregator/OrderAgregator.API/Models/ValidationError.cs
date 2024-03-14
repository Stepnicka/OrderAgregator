using FluentValidation.Results;

namespace OrderAgregator.API.Models
{
    public record ValidationError
    {
        public required List<ValidationFailure> Errors { get; init; }
    }
}
