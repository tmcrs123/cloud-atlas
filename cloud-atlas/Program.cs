
using System.Text.Json.Serialization;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.SecretsManager;
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
            builder.ConfigureAuthorization();
            builder.ConfigureGlobalErrorHandling();
            builder.ConfigureResponseCompression();
            builder.ConfigureCors();
            builder.ConfigureDatabase();

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add(new AuthorizeFilter());

            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAuthorization();
            builder.Services.AddDefaultAWSOptions(new AWSOptions()
            {
                Region = RegionEndpoint.USEast1
            });
            builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
            builder.Services.AddAWSService<IAmazonS3>();
            builder.Services.AddAWSService<IAmazonDynamoDB>();
            builder.Services.AddAWSService<IAmazonSecretsManager>();
            builder.Services.AddScoped<IDynamoDBContext>(provider =>
            {
                var dynamoDbClient = new AmazonDynamoDBClient(RegionEndpoint.USEast1);

                var ddb = new DynamoDBContextBuilder();

                ddb.ConfigureContext(config =>
                {
                    config.TableNamePrefix = "cloud-atlas-demo-";
                });

                ddb.WithDynamoDBClient(() => new AmazonDynamoDBClient(RegionEndpoint.USEast1));

                return ddb.Build();
            });

            builder.Services.AddHttpContextAccessor();
            
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
