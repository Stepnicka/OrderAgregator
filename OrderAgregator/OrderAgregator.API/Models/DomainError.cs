namespace OrderAgregator.API.Models
{
    public class DomainError : Of<ValidationError, InternalError>
    {
        protected DomainError(ValidationError value) : base(value)
        {
        }

        protected DomainError(InternalError value) : base(value)
        {
        }

        public static implicit operator DomainError(ValidationError error) => new(error);
        public static implicit operator DomainError(InternalError error) => new(error);
    }
}
