namespace SlotFlow.Api.Domain.Exceptions
{
    public sealed class DomainException(Domain.Errors.Error error) : Exception(error.Message)
    {
        public Errors.Error Error { get; } = error;
    }
}
