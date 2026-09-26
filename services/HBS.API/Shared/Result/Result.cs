
using HBS.API.Shared.enums;

namespace HBS.API.Shared.Result;

public record Success();

public class Result : IResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<Error> Error { get; }
    public int StatusCode { get; }

    protected Result(bool isSuccess, IEnumerable<Error> errors)
    {
        IsSuccess = isSuccess;
        Error = errors.ToList().AsReadOnly();
        StatusCode = MapToStatusCode(isSuccess, Error.FirstOrDefault()?.Type);
    }

    public static Result Success() => new Result(true, Array.Empty<Error>());
    public static Result Failure(Error error) => new Result(false, new[] { error });
    public static Result Failure(IEnumerable<Error> errors) => new Result(false, errors);

    private static int MapToStatusCode(bool isSuccess, ErrorKind? type) =>
        isSuccess ? 200 : type switch
        {
            ErrorKind.Validation => 400,
            ErrorKind.NotFound => 404,
            ErrorKind.Conflict => 409,
            ErrorKind.Unauthorized => 401,
            _ => 500
        };
}

public sealed class Result<TValue> : Result, IResult<TValue>
{
    private readonly TValue? _value;

    private Result(TValue? value, bool isSuccess, IEnumerable<Error> errors)
        : base(isSuccess, errors)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Failure results cannot have a value.");

    public TOut Match<TOut>(Func<TOut> onSuccess, Func<IReadOnlyList<Error>, TOut> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Error);
    }

    public static Result<TValue> Success(TValue value) => new(value, true, Array.Empty<Error>());
    public new static Result<TValue> Failure(Error error) => new(default, false, new[] { error });
    public new static Result<TValue> Failure(IEnumerable<Error> errors) => new(default, false, errors);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}