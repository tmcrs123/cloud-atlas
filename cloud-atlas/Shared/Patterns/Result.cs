using cloud_atlas.Shared.Errors;

namespace cloud_atlas.Shared.Patterns
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public ApplicationError? Error { get; set; }
        public bool isFailure => !IsSuccess;

        public Result(bool isSuccess, ApplicationError? error)
        {
            IsSuccess = isSuccess;
            Error = error ?? null;

            if (isFailure && error is null)
            {
                throw new InvalidOperationException("Failure must have an error");
            }

            if (isSuccess && error is not null)
            {
                throw new InvalidOperationException("Success cannot have an error");
            }
        }

        public static Result Success()
        {
            return new Result(true, null);
        }

        public static Result Failure(ApplicationError error)
        {
            return new Result(false, error);
        }
    }

    public class Result<T> : Result
    {
        public T Value { get; set; }

        public Result(T value, bool isSuccess, ApplicationError? error) : base(isSuccess, error)
        {
            Value = value;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(value, true, null);
        }

        public static new Result<T> Failure(ApplicationError error)
        {
            return new Result<T>(default, false, error);
        }
    }
}
