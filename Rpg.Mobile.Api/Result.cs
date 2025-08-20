namespace Rpg.Mobile.Api;

public class NoResult
{
    public static NoResult Value { get; } = new();
}

public class Result<TError> : Result<NoResult, TError>
{
}

public class Result<TValue, TError>
{
    public TValue? Value { get; set; }
    public TError? Error { get; set; }
}