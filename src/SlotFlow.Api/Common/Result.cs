namespace SlotFlow.Api.Common
{
    public sealed class Result<T>
    {
        private Result(T value)
        {
            IsSuccess = true;
            Value = value;
            Error = Domain.Errors.Error.None;
        }

        private Result(Domain.Errors.Error error)
        {
            IsSuccess = false;
            Error = error;
            Value = default;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T? Value { get; }
        public Domain.Errors.Error Error { get; }

        public static Result<T> Success(T value) => new(value);
        public static Result<T> Failure(Domain.Errors.Error error) => new(error);
    }
}
