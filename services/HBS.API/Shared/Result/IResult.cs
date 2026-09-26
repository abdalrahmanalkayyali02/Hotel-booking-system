namespace HBS.API.Shared.Result;

public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    IReadOnlyList<Error> Error { get; }
    int StatusCode { get; }
}

public interface IResult<out TValue> : IResult
{
    TValue Value { get; }
}