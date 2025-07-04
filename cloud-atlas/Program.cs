
using Microsoft.AspNetCore.Mvc.Authorization;

namespace cloud_atlas
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.ConfigureEnvironment();
            builder.ConfigureGlobalErrorHandling();
            builder.ConfigureResponseCompression();
            builder.ConfigureCors();
            builder.ConfigureDatabase();

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add(new AuthorizeFilter());
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAuthorization();
            builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
            builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");
            app.UseAuthorization();
            app.UseMiddleware<RequireSubClaimMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}
