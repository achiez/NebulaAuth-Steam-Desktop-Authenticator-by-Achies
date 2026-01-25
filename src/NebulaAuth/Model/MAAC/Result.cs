using System;

namespace NebulaAuth.Model.MAAC;

public struct Result<T>
{
    public bool IsSuccess { get; }
    public T Data => !IsSuccess ? throw new InvalidOperationException("No data available") : _data!;
    public Exception? Exception { get; }

    private readonly T? _data;

    private Result(bool success, T? data, Exception? exception)
    {
        IsSuccess = success;
        _data = data;
        Exception = exception;
    }

    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data, null);
    }

    public static Result<T> Error(Exception ex)
    {
        return new Result<T>(false, default, ex);
    }
}