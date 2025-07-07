using Amazon.Runtime.Internal.Auth;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using cloud_atlas;
using cloud_atlas.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class PhotosController : BaseController
{
    private readonly SqlDbContext SqlDbContext;
    private readonly CosmosDbContext CosmosDbContext;
    private readonly IAmazonS3 S3Client;
    private IAmazonSecretsManager SecretsManagerClient { get; set; }
    private IConfiguration Configuration { get; set; }

    public PhotosController(CosmosDbContext cosmosDbContext, SqlDbContext sqlDbContext, IAmazonS3 s3Client, IAmazonSecretsManager secretsManagerClient, IConfiguration configuration)
    {
        CosmosDbContext = cosmosDbContext;
        SqlDbContext = sqlDbContext;
        S3Client = s3Client;
        SecretsManagerClient = secretsManagerClient;
        Configuration = configuration;
    }

    [HttpGet("cloudfront-url")]
    public async Task<IActionResult> GetCloudfrontSignedURL([FromQuery] Guid? photoLinkId)
    {

        string path = Configuration.GetValue<string>("AWS:TmpPath");
        string xmlSecret;

        if (System.IO.File.Exists(path))
        {
            System.Console.WriteLine("File exists at path {0}", path);
            xmlSecret = System.IO.File.ReadAllText(path);
        }
        else
        {
            System.Console.WriteLine("File DOES NOT EXIST at path {0}", path);
            // generate key in xml
            GetSecretValueRequest request = new GetSecretValueRequest()
            {
                SecretId = Configuration.GetValue<string>("AWS:CloudfrontPrivateKeyName")
            };

            var secret = await SecretsManagerClient.GetSecretValueAsync(request);

            xmlSecret = CloudfrontSignedURLUtils.ConvertPemToXML(secret.SecretString);

            System.IO.File.WriteAllText(path, xmlSecret);
        }

        var cloudfrontKeyPairId = Configuration.GetValue<string>("AWS:CloudfrontKeyPairId");
        var cloudfrontDomain = Configuration.GetValue<string>("AWS:CloudfrontDomain");
        var objectKey = "017895e8-dbc1-4f03-9c9c-12e1671c807c.jpg"; // or photo.S3Key
        var s3Url = $"https://{cloudfrontDomain}/{objectKey}";
        var expiresOn = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
        var policy = $"{{\"Statement\":[{{\"Resource\":\"{s3Url}\",\"Condition\":{{\"DateLessThan\":{{\"AWS:EpochTime\":{expiresOn}}}}}}}]}}";


        var url = CloudfrontSignedURLUtils.CreateCannedPrivateURL(s3Url, "minutes", "5", policy, xmlSecret, cloudfrontKeyPairId);

        return Ok(url);
    }

    [HttpPost("presigned-url")]
    public IActionResult CreatePresignedURL([FromBody] CreatePresignedURLDto dto)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = Configuration.GetValue<string>("AWS:DumpBucketName"),
            // Key = $"{dto.AtlasId}/{dto.MarkerId}/{Guid.NewGuid()}",
            Key = "017895e8-dbc1-4f03-9c9c-12e1671c807c.jpg",
            Verb = HttpVerb.PUT,
            Expires = DateTime.Now.AddMinutes(60)
        };

        string url = S3Client.GetPreSignedURL(request);
        return Ok(url);
    }

    [HttpGet("s3-url")]
    public async Task<IActionResult> GetPhotosForMarker([FromQuery] Guid markerId)
    {
        var photos = await CosmosDbContext.MarkerPhotos.AsNoTracking().Where(mp => mp.MarkerId == markerId).Select(mp => mp.Photos).ToListAsync();
        return Ok(photos);
    }

    [HttpPost]
    public async Task<IActionResult> SavePhotos([FromBody] SavePhotosDto dto)
    {
        var photoLinkId = await SqlDbContext.PhotoLinks.FirstOrDefaultAsync(pl => pl.MarkerId == dto.MarkerId);

        if (photoLinkId is null) return NotFound();

        //does this marker already have photos?
        var existingMarker = await CosmosDbContext.MarkerPhotos.FirstOrDefaultAsync(mp => mp.MarkerId == dto.MarkerId);

        if (existingMarker is null)
        {
            MarkerPhotos markerPhotos = new MarkerPhotos()
            {
                AtlasId = dto.AtlasId,
                MarkerId = dto.MarkerId,
                Photos = dto.PhotosData,
                PhotoLinkId = photoLinkId.PhotoLinkId
            };

            CosmosDbContext.MarkerPhotos.AddRange(markerPhotos);
        }
        else
        {
            existingMarker.Photos.AddRange(dto.PhotosData);
        }

        await CosmosDbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePhoto([FromBody] UpdatePhotoDto dto)
    {
        var photoLink = await CosmosDbContext.MarkerPhotos
            .FirstOrDefaultAsync(pl => pl.PhotoLinkId == dto.PhotoLinkId);

        if (photoLink == null)
            return NotFound();

        var photo = photoLink.Photos.FirstOrDefault(p => p.Id == dto.PhotoData.Id);

        if (photo == null)
            return NotFound();

        photo.Legend = dto.PhotoData.Legend;

        await CosmosDbContext.SaveChangesAsync();

        return Ok(photo);
    }

    [HttpDelete]
    public async Task<IActionResult> DeletePhoto([FromBody] DeletePhotoDto dto)
    {
        var photoLink = await CosmosDbContext.MarkerPhotos
            .FirstOrDefaultAsync(pl => pl.PhotoLinkId == dto.PhotoLinkId);

        if (photoLink == null)
            return NotFound();

        photoLink.Photos = photoLink.Photos.Where(p => p.Id != dto.PhotoId).ToList();

        await CosmosDbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("delete-photo-from-bucket")]
    public async Task<IActionResult> DeletePhotoFromBucket([FromBody] string key)
    {
        try
        {
            var bucketName = this.HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["AWS:BucketName"];

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await S3Client.DeleteObjectAsync(deleteRequest);

            return Ok($"Deleted '{key}' from bucket '{bucketName}'.");
        }
        catch (System.Exception e)
        {
            System.Console.WriteLine(e);
            throw;
        }
    }
}