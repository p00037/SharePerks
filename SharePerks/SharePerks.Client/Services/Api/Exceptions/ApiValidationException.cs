namespace Shareholder.Client.Services.Api.Exceptions;

public sealed class ApiValidationException : Exception
{
    public ApiValidationException(string message, IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
