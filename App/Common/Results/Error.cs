namespace Pinkterest.Application.Common.Results;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string entity) => new($"{entity}.NotFound", $"{entity} was not found.");

    public static Error Forbidden(string action) => new($"{action}.Forbidden", $"You are not allowed to {action}.");

    public static Error Validation(string code, string message) => new(code, message);
}
