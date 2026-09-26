using System.Diagnostics;
using HBS.API.Shared.enums;

namespace HBS.API.Shared.Result;

public record Error
{
    public string Code { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public ErrorKind Type { get; init; }
    public DateTime Timestamp { get; init; }
    public string TraceId { get; init; }
    public Dictionary<string, object>? Extensions { get; init; }

    private Error(
        string code,
        string title,
        string description,
        ErrorKind type,
        Dictionary<string, object>? extensions = null)
    {
        Code = code;
        Title = title;
        Description = description;
        Type = type;
        Extensions = extensions;

        Timestamp = DateTime.UtcNow;

        TraceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
    }

    public static Error NotFound(string code, string description)
        => new(code, "Entity Not Found", description, ErrorKind.NotFound);

    public static Error Validation(string code, string description, Dictionary<string, object>? errors = null)
        => new(code, "Validation Error", description, ErrorKind.Validation, errors);

    public static Error Conflict(string code, string description)
        => new(code, "Conflict Occurred", description, ErrorKind.Conflict);

    public static Error Unauthorized(string code, string description)
        => new(code, "Unauthorized Access", description, ErrorKind.Unauthorized);

    public static Error Failure(string code, string description)
        => new(code, "General Failure", description, ErrorKind.Failure);

    public static readonly Error None = new(string.Empty, string.Empty, string.Empty, ErrorKind.Failure);
}