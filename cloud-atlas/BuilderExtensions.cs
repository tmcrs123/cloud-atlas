using cloud_atlas.Config;
using cloud_atlas.Shared.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

namespace cloud_atlas
{
    public static class BuilderExtensions
    {

        public static void ConfigureAuthorization(this WebApplicationBuilder builder)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrEmpty(env))
            {
                throw new Exception("Configuration not found for ASPNETCORE_ENVIRONMENT");
            }

            if (env.ToLower() == "development")
            {
                builder.Services.AddAuthentication("FakeAuth")
                    .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>("FakeAuth", null);
            }
        }

        public static void ConfigureEnvironment(this WebApplicationBuilder builder)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrEmpty(env))
            {
                throw new Exception("Configuration not found for ASPNETCORE_ENVIRONMENT");
            }

            builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile($"appsettings.{env.ToLower()}.json");
        }

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
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
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

        public static void ConfigureDatabase(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<CosmosDbSettings>(builder.Configuration.GetSection("CosmosDb"));

            builder.Services.Configure<SqlDbSettings>(builder.Configuration.GetSection("SqlDb"));

            var cosmosDbSettings = builder.Configuration.GetSection("CosmosDb").Get<CosmosDbSettings>();
            var sqlDbSettings = builder.Configuration.GetSection("SqlDb").Get<SqlDbSettings>();

            builder.Services.AddDbContext<SqlDbContext>(options =>
            {
                options.UseSqlServer(sqlDbSettings?.ConnectionString, sqlServer =>
                {
                    sqlServer.UseNetTopologySuite();
                    sqlServer.EnableRetryOnFailure();
                });
            });
        }
    }
}
