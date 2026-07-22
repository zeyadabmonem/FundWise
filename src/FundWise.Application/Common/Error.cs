namespace FundWise.Application.Common;

/// <summary>
/// Represents a typed error with a code and description.
/// Used with Result&lt;T&gt; to avoid throwing exceptions for expected failures.
/// </summary>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    public static Error NotFound(string entity, object key) =>
        new($"{entity}.NotFound", $"{entity} with key '{key}' was not found.");

    public static Error Conflict(string code, string description) =>
        new(code, description);

    public static Error Unauthorized(string description = "Unauthorized.") =>
        new("Error.Unauthorized", description);

    public static Error Validation(string field, string description) =>
        new($"Validation.{field}", description);

    public static Error External(string service, string description) =>
        new($"External.{service}", description);

    public override string ToString() => $"[{Code}] {Description}";
}
