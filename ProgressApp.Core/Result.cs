using System;
namespace ProgressApp.Core {

public class
Result {
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Exception? Exception { get; }
    public long BytesReceived { get; }

    protected Result(bool isSuccess, Exception? exception = null, long bytesReceived = 0) {
        IsSuccess = isSuccess;
        Exception = exception;
        BytesReceived = bytesReceived;
    }

    public static Result
    Success() => new Result(true);

    public static Result
    Failure(long bytesReceived, Exception? exception = null) => new Result(false, exception, bytesReceived);
}

public static class
ResultEx {
    public static Result
    OnSuccess(this Result result, Func<Result> func) => result.IsFailure ? result : func();

    public static Result
    OnSuccess(this Result result, Action action) {
        if (result.IsFailure)
            return result;
        action();
        return Result.Success();
    }

    public static Result
    OnFailure(this Result result, Action action) {
        if (result.IsFailure)
            action();
        return  result;
    }

    public static Result
    OnFailure(this Result result, Action<Result> action, Func<Result, bool> predicate) {
        if (result.IsFailure && predicate(result))
            action(result);
        return  result;
    }

    public static Result
    OnFailure(this Result result, Action action, Func<Result, bool> predicate) {
        if (result.IsFailure && predicate(result))
            action();
        return  result;
    }

    public static Result
    OnFailure(this Result result, Action<Result> action) {
        if (result.IsFailure)
            action(result);
        return  result;
    }

    public static Result
    OnBoth(this Result result, Action<Result> action) {
        action(result);
        return result;
    }
}
}
