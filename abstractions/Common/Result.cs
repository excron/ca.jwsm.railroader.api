namespace Ca.Jwsm.Railroader.Api.Abstractions.Common
{
    public class Result
    {
        protected Result(bool succeeded, string errorMessage)
        {
            Succeeded = succeeded;
            ErrorMessage = errorMessage;
        }

        public bool Succeeded { get; }

        public string ErrorMessage { get; }

        public static Result Success()
        {
            return new Result(true, null);
        }

        public static Result Failure(string errorMessage)
        {
            return new Result(false, errorMessage);
        }
    }

    public sealed class Result<T> : Result
    {
        private Result(bool succeeded, T value, string errorMessage)
            : base(succeeded, errorMessage)
        {
            Value = value;
        }

        public T Value { get; }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, null);
        }

        public new static Result<T> Failure(string errorMessage)
        {
            return new Result<T>(false, default(T), errorMessage);
        }
    }
}
