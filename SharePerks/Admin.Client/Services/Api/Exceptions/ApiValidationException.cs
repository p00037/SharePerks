namespace Admin.Client.Services.Api.Exceptions
{
    public class ApiValidationException : Exception
    {
        public ApiValidationException(string message, IReadOnlyDictionary<string, string[]> errors)
            : base(message)
        {
            Errors = errors;
        }

        public IReadOnlyDictionary<string, string[]> Errors { get; }
    }
}
