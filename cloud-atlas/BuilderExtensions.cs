using cloud_atlas.Shared.Exceptions;
using Microsoft.AspNetCore.ResponseCompression;

namespace cloud_atlas
{
    public static class BuilderExtensions
    {
        public static void ConfigureGlobalErrorHandling(this WebApplicationBuilder builder)
        {
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        }

        public static void ConfigureCors(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                    .WithOrigins(["*"])
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .AllowAnyMethod();
                });
            });
        }

        public static void ConfigureResponseCompression(this WebApplicationBuilder builder)
        {
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes;
            });
        }

    }
}
