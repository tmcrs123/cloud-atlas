using Microsoft.AspNetCore.Diagnostics;

namespace cloud_atlas.Shared.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        ValueTask<bool> IExceptionHandler.TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
