namespace LvJerseyApi.Exceptions;

public record ResponseException(
    int StatusCode,
    string Messages,
    IDictionary<string, string[]>? Errors = null
);