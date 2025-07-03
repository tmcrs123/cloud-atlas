public class RequireSubClaimMiddleware
{
    private readonly RequestDelegate _next;

    public RequireSubClaimMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var subClaim = context.User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(subClaim))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Missing 'sub' claim.");
            return;
        }

        context.Items["sub"] = subClaim;

        await _next(context);
    }
}
