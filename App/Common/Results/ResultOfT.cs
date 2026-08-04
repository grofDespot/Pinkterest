namespace Pinkterest.Application.Common.Results;

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error) => _value = value;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

    public Result<TOut> Map<TOut>(Func<TValue, TOut> map) =>
        IsSuccess ? Success(map(Value)) : Failure<TOut>(Error);

    public Result<TOut> Bind<TOut>(Func<TValue, Result<TOut>> bind) =>
        IsSuccess ? bind(Value) : Failure<TOut>(Error);

    public Result<TValue> Ensure(Func<TValue, bool> predicate, Error error) =>
        IsFailure || predicate(Value) ? this : Failure<TValue>(error);

    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);
}
