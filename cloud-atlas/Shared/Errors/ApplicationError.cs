namespace cloud_atlas.Shared.Errors
{
    public class ApplicationError
    {
        public string Message { get; set; }
        public ErrorType ErrorType { get; set; }

        public ApplicationError(string message, ErrorType errorType)
        {
            Message = message;
            ErrorType = errorType;
        }
    }
}
