namespace IdentityService.Application.Models;

public class Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public List<string> Errors { get; init; } = new();

    public static Result<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value
    };

    public static Result<T> Failure(params string[] errors) => new()
    {
        IsSuccess = false,
        Errors = errors.ToList()
    };
}
