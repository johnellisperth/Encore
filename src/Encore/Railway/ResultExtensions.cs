
namespace Encore.Railway;

public static class ResultExtensions
{
    /*public static Result OnSucc(this Result resultTask, Func<Result> func)
    {
        var result =  resultTask;
        if (!result.IsSuccess)
            return result;

        return func();
    }*/

    public static async Task<Result> OnSuccess(this Task<Result> resultTask, Func<Task<Result>> func)
    {
        var result = await resultTask;
        if (!result.IsSuccess)
            return result;

        return await func();
    }

    public static async Task<Result<TOut>> OnSuccess<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<Result<TOut>>> func)
    {
        var result = await resultTask;
        if (!result.IsSuccess)
            return Result<TOut>.Failure(result.Error);

        return await func(result.Value);
    }

    public static async Task<Result> OnFailure(this Task<Result> resultTask, Func<string, Task<Result>> func)
    {
        var result = await resultTask;
        if (result.IsSuccess)
            return result;

        return await func(result.Error);
    }

    public static async Task<Result<T>> OnFailure<T>(this Task<Result<T>> resultTask, Func<string, Task<Result<T>>> func)
    {
        var result = await resultTask;
        if (result.IsSuccess)
            return result;

        return await func(result.Error);
    }

    public static Result Try(Action action)
    {
        try
        {
            action();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public static Result<T> Try<T>(Func<T> func)
    {
        try
        {
            return Result<T>.Success(func());
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex.Message);
        }
    }

    public static async Task<Result> TryAsync(Func<Task> func)
    {
        try
        {
            await func();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func)
    {
        try
        {
            T result = await func();
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex.Message);
        }
    }
}
