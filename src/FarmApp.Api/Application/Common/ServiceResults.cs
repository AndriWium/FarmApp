namespace FarmApp.Api.Application.Common;

/// <summary>Business-rule outcomes a service can report without throwing.
/// The Presentation layer maps these to HTTP statuses in one place (ApiControllerBase).</summary>
public enum ServiceError
{
    None,
    NotFound,
    DuplicateName,
}

/// <summary>A service result carrying either a value (Error == None) or a business error.</summary>
public record ServiceResult<T>(T? Value, ServiceError Error)
{
    public static ServiceResult<T> Ok(T value) => new(value, ServiceError.None);
    public static ServiceResult<T> Fail(ServiceError error) => new(default, error);
}
