using cloud_atlas.Config;
using cloud_atlas.Shared.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
                    .WithOrigins("http://localhost:4200")
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

        public static void ConfigureDatabase(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<CosmosDbSettings>(builder.Configuration.GetSection("CosmosDb"));

            builder.Services.Configure<SqlDbSettings>(builder.Configuration.GetSection("SqlDb"));

            var cosmosDbSettings = builder.Configuration.GetSection("CosmosDb").Get<CosmosDbSettings>();
            var sqlDbSettings = builder.Configuration.GetSection("SqlDb").Get<SqlDbSettings>();

            builder.Services.AddDbContext<SqlDbContext>(options =>
            {
                options.UseSqlServer(sqlDbSettings?.ConnectionString, sqlServer => sqlServer.UseNetTopologySuite());
            });

            builder.Services.AddDbContext<CosmosDbContext>(options =>
            {
                options.UseCosmos(cosmosDbSettings.URL, cosmosDbSettings.Key, databaseName: cosmosDbSettings.DatabaseName);
            });
        }

        public static void ConfigureAuthentication(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                // options.Authority = $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:Authority"]}/v2.0";
                options.Authority = "https://cloudatlastest.ciamlogin.com/ab3dfd14-7454-472f-99a9-13359bf26e19/.well-known/openid-configuration";
                options.Audience = "e5850140-95d8-46f8-9cea-a392fe579a0e";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    // ValidIssuer = "https://sts.windows.net/ab3dfd14-7454-472f-99a9-13359bf26e19/",
                    ValidIssuer = "https://ab3dfd14-7454-472f-99a9-13359bf26e19.ciamlogin.com/ab3dfd14-7454-472f-99a9-13359bf26e19/v2.0",
                    ValidateAudience = true,
                    ValidateLifetime = true
                };
            });
        }
    }
}
