
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace cloud_atlas
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.ConfigureGlobalErrorHandling();
            builder.ConfigureResponseCompression();
            builder.ConfigureCors();
            builder.ConfigureDatabase();
            builder.ConfigureAuthentication();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseCors("AllowAll");

            app.MapControllers();

            app.Run();
        }
    }
}
