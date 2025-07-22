using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace cloud_atlas.Shared.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        ValueTask<bool> IExceptionHandler.TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            System.Console.WriteLine(exception);
            System.Console.WriteLine(JsonSerializer.Serialize(exception.StackTrace));
            throw exception;
        }
    }
}
