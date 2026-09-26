namespace HBS.API.Shared.enums;

public enum ErrorKind
{
    Failure = 0,      // 500 Internal Server Error
    Unexpected = 1,   // 500
    Validation = 2,   // 400 Bad Request
    Conflict = 3,     // 409 Conflict
    NotFound = 4,     // 404 Not Found
    Unauthorized = 5, // 401 Unauthorized
    Forbidden = 6     // 403 Forbidden
}