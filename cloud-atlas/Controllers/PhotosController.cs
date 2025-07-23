using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using cloud_atlas.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class PhotosController : BaseController
{
    private readonly IAmazonS3 S3Client;
    private IAmazonSecretsManager SecretsManagerClient { get; set; }
    public IDynamoDBContext DynamoDBContext { get; set; }
    private IConfiguration Configuration { get; set; }

    public PhotosController(IAmazonS3 s3Client, IAmazonSecretsManager secretsManagerClient, IConfiguration configuration, IDynamoDBContext dynamoDBContext)
    {
        S3Client = s3Client;
        SecretsManagerClient = secretsManagerClient;
        Configuration = configuration;
        DynamoDBContext = dynamoDBContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetCloudfrontSignedURL([FromQuery] Guid AtlasId, [FromQuery] Guid MarkerId)
    {
        var response = await DynamoDBContext.LoadAsync<MarkerPhotos>(AtlasId.ToString(), MarkerId.ToString());

        if(response is null) return Ok(new List<string>());

        if (!response.Photos.Any()) return Ok(new List<string>());

        string xmlSecret;

        GetSecretValueRequest request = new GetSecretValueRequest()
        {
            SecretId = Configuration.GetValue<string>("AWS:CloudfrontPrivateKeyName")
        };

        var secret = await SecretsManagerClient.GetSecretValueAsync(request);

        xmlSecret = CloudfrontSignedURLUtils.ConvertPemToXML(secret.SecretString);

        List<object> photosData = new List<object>();
        var cloudfrontKeyPairId = Configuration.GetValue<string>("AWS:CloudfrontKeyPairId");
        var cloudfrontDomain = Configuration.GetValue<string>("AWS:CloudfrontDomain");
        var expiresOn = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();

        foreach (var photo in response.Photos)
        {
            var objectKey = $"{AtlasId}/{MarkerId}/{photo.Id}";
            var s3Url = $"https://{cloudfrontDomain}/{objectKey}";
            var policy = $"{{\"Statement\":[{{\"Resource\":\"{s3Url}\",\"Condition\":{{\"DateLessThan\":{{\"AWS:EpochTime\":{expiresOn}}}}}}}]}}";
            var url = CloudfrontSignedURLUtils.CreateCannedPrivateURL(s3Url, "minutes", "5", policy, xmlSecret, cloudfrontKeyPairId);

            photosData.Add(new { url, photo.Legend, photo.Id  });
        }

        return Ok(photosData);
    }

    [HttpPost("presigned-url")]
    public IActionResult CreatePresignedURL([FromBody] CreatePresignedURLDto dto)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = Configuration.GetValue<string>("AWS:DumpBucketName"),
            Key = $"{dto.AtlasId}/{dto.MarkerId}/{dto.Filename}",
            Verb = HttpVerb.PUT,
            Expires = DateTime.Now.AddMinutes(65)
        };

        string url = S3Client.GetPreSignedURL(request);
        return Ok(url);
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePhotoDynamo([FromBody] UpdatePhotoDto dto)
    {
        var response = await DynamoDBContext.LoadAsync<MarkerPhotos>(
            dto.AtlasId.ToString(),
            dto.MarkerId.ToString()
        );

        if (!response.Photos.Any())
        {
            return NotFound();
        }

        var photoToUpdate = response.Photos.Where(p => p.Id == dto.PhotoData.Id).SingleOrDefault();

        if (photoToUpdate is null) return NotFound();

        photoToUpdate.Legend = dto.PhotoData.Legend;

        await DynamoDBContext.SaveAsync(response);

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeletePhoto([FromBody] DeletePhotoDto dto)
    {
        var response = await DynamoDBContext.LoadAsync<MarkerPhotos>(
            dto.AtlasId.ToString(),
            dto.MarkerId.ToString()
        );

        if (!response.Photos.Any())
        {
            return NotFound();
        }

        if (!response.Photos.Where(p => p.Id == dto.PhotoId).Any())
        {
            return NotFound();
        }

        response.Photos.RemoveAll(p => p.Id == dto.PhotoId);

        await DynamoDBContext.SaveAsync(response);

        var bucketName = HttpContext.RequestServices
                        .GetRequiredService<IConfiguration>()["AWS:BucketName"];

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = $"{dto.AtlasId}/{dto.MarkerId}/{dto.PhotoId}"
        };

        var s3Response = await S3Client.DeleteObjectAsync(deleteRequest);

        return Ok();
    }

    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAllPhotosForMarker([FromBody] DeleteAllPhotosDto dto)
    {
        var response = await DynamoDBContext.LoadAsync<MarkerPhotos>(
            dto.AtlasId.ToString(),
            dto.MarkerId.ToString()
        );

        if (response is null)
        {
            return NotFound();
        }

        await DynamoDBContext.DeleteAsync(response);

        var bucketName = HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()["AWS:BucketName"];

        var listRequest = new ListObjectsV2Request
        {
            BucketName = bucketName,
            Prefix = $"{dto.AtlasId}/{dto.MarkerId}/"
        };

        var listResponse = await S3Client.ListObjectsV2Async(listRequest);

        if (listResponse.S3Objects.Any())
        {
            var deleteObjectsRequest = new DeleteObjectsRequest
            {
                BucketName = bucketName,
                Objects = listResponse.S3Objects
                    .Select(o => new KeyVersion { Key = o.Key })
                    .ToList()
            };

            await S3Client.DeleteObjectsAsync(deleteObjectsRequest);
        }

        return Ok();
    }
}