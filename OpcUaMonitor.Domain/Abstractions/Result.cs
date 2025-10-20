using System.Diagnostics.CodeAnalysis;

namespace OpcUaMonitor.Domain.Abstractions;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    
    public Error? Error { get; }

    public Result(bool isSuccess, Error? error)
    {
        switch (isSuccess)
        {
            case true when error != Error.None:
                throw new InvalidOperationException("成功的结果不能包含错误。");
            case false when error == Error.None:
                throw new InvalidOperationException("失败的结果必须包含错误。");
            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }
    
    public static Result Success() => new Result(true, Error.None);
    
    public static Result Failure(Error error) => new Result(false, error);
    
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }
    
    [NotNull]
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("无法访问失败结果的值。");

    public static implicit operator Result<T>(T? value) => Create(value);
}