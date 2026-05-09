namespace TaskManager.Application.Common;

public sealed class Result
{
    private Result(bool succeeded, string? error)
    {
        Succeeded = succeeded;
        Error = error;
    }

    public bool Succeeded { get; }

    public string? Error { get; }

    public static Result Ok() => new(true, null);

    public static Result Fail(string error) => new(false, error);
}

public sealed class Result<T>
{
    private Result(bool succeeded, T? value, string? error)
    {
        Succeeded = succeeded;
        Value = value;
        Error = error;
    }

    public bool Succeeded { get; }

    public T? Value { get; }

    public string? Error { get; }

    public static Result<T> Ok(T value) => new(true, value, null);

    public static Result<T> Fail(string error) => new(false, default, error);
}
