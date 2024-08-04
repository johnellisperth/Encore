
namespace Encore.Railway;

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new Result(true, string.Empty);
    public static Result Failure(string error) => new Result(false, error);
}

public class Result<T> : Result
{
    public T Value { get; }

    protected Result(T value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static new Result<T> Success(T value) => new Result<T>(value, true, string.Empty);
    public static new Result<T> Failure(string error) => new Result<T>(default, false, error);
}

public class Option<T> where T : class
{
    private T? Object_ = null;
    public static Option<T> Some(T obj) => new() { Object_ = obj };
    public static Option<T> None() => new ();

    public  Option<Result> Map<Result>(Func<T, Result> map) where Result : class =>
        Object_ is null ? Option<Result>.None() : Option<Result>.Some(map(Object_));
    public T Reduce(T @default) => Object_ ?? default;
}

