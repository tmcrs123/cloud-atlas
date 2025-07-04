using Amazon.CloudFront;
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
    private readonly SqlDbContext sqlDbContext;
    private readonly CosmosDbContext cosmosDbContext;
    private readonly IAmazonS3 s3Client;
    private IAmazonSecretsManager secretsManagerClient { get; set; }
    private IAmazonCloudFront CloudfrontClient { get; set; }
    private IConfiguration Configuration { get; set; }

    public PhotosController(CosmosDbContext cosmosDbContext, SqlDbContext sqlDbContext, IAmazonS3 s3Client, IAmazonSecretsManager secretsManagerClient, IConfiguration configuration, IAmazonCloudFront cloudfrontClient)
    {
        this.cosmosDbContext = cosmosDbContext;
        this.sqlDbContext = sqlDbContext;
        this.s3Client = s3Client;
        this.secretsManagerClient = secretsManagerClient;
        Configuration = configuration;
        this.CloudfrontClient = cloudfrontClient;
    }

    [HttpPost("presigned-url")]
    public IActionResult CreatePresignedURL([FromBody] CreatePresignedURLDto dto)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = Configuration.GetValue<string>("AWS:BucketName"),
            // Key = $"{dto.AtlasId}/{dto.MarkerId}/{Guid.NewGuid()}",
            Key = "017895e8-dbc1-4f03-9c9c-12e1671c807c.jpg",
            Verb = HttpVerb.PUT,
            Expires = DateTime.Now.AddMinutes(60)
        };

        string url = s3Client.GetPreSignedURL(request);
        return Ok(url);
    }

    [HttpGet("presigned-url")]
    // public async Task<IActionResult> GetPhotoUrlsFromCloudfront([FromQuery]signedUrl Guid photoLinkId)
    public async Task<IActionResult> GetPhotoUrlsFromCloudfront()
    {
        // var details = await cosmosDbContext.MarkerPhotos.FirstOrDefaultAsync(mp => mp.PhotoLinkId == photoLinkId);
        // var photos = details.Photos;

        GetSecretValueRequest request = new GetSecretValueRequest()
        {
            SecretId = Configuration.GetValue<string>("AWS:CloudfrontPrivateKeyName")
        };

        var secret = await secretsManagerClient.GetSecretValueAsync(request);

        var cloudfrontKeyPairId = Configuration.GetValue<string>("AWS:CloudfrontKeyPairId");
        var cloudfrontDomain = Configuration.GetValue<string>("AWS:CloudfrontDomain");

        // The secret should contain the private key in PEM format
        var privateKeyPem = secret.SecretString;
        var presignedUrls = new List<string>();

        var objectKey = "017895e8-dbc1-4f03-9c9c-12e1671c807c.jpg"; // or photo.S3Key
        var s3Url = $"https://{cloudfrontDomain}/{objectKey}";
        var expiresOn = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
        var policy = $"{{\"Statement\":[{{\"Resource\":\"{s3Url}\",\"Condition\":{{\"DateLessThan\":{{\"AWS:EpochTime\":{expiresOn}}}}}}}]}}";

        // Sign the policy using RSA SHA1
        string signature;
        using (var rsa = System.Security.Cryptography.RSA.Create())
        {
            rsa.ImportFromPem(privateKeyPem.ToCharArray());
            var data = System.Text.Encoding.UTF8.GetBytes(policy);
            var signedBytes = rsa.SignData(data, System.Security.Cryptography.HashAlgorithmName.SHA1, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
            signature = System.Convert.ToBase64String(signedBytes).Replace('+', '-').Replace('=', '_').Replace('/', '~');
        }

        var signedUrl = $"{s3Url}?Expires={expiresOn}&Signature={signature}&Key-Pair-Id={cloudfrontKeyPairId}";
        presignedUrls.Add(signedUrl);

        return Ok(signedUrl);
    }

    [HttpGet]
    public async Task<IActionResult> GetPhotosForMarker([FromQuery] Guid markerId)
    {
        var photos = await cosmosDbContext.MarkerPhotos.AsNoTracking().Where(mp => mp.MarkerId == markerId).Select(mp => mp.Photos).ToListAsync();
        return Ok(photos);
    }

    [HttpPost]
    public async Task<IActionResult> SavePhotos([FromBody] SavePhotosDto dto)
    {
        var photoLinkId = await sqlDbContext.PhotoLinks.FirstOrDefaultAsync(pl => pl.MarkerId == dto.MarkerId);

        if (photoLinkId is null) return NotFound();

        //does this marker already have photos?
        var existingMarker = await cosmosDbContext.MarkerPhotos.FirstOrDefaultAsync(mp => mp.MarkerId == dto.MarkerId);

        if (existingMarker is null)
        {
            MarkerPhotos markerPhotos = new MarkerPhotos()
            {
                AtlasId = dto.AtlasId,
                MarkerId = dto.MarkerId,
                Photos = dto.PhotosData,
                PhotoLinkId = photoLinkId.PhotoLinkId
            };

            cosmosDbContext.MarkerPhotos.AddRange(markerPhotos);
        }
        else
        {
            existingMarker.Photos.AddRange(dto.PhotosData);
        }

        await cosmosDbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePhoto([FromBody] UpdatePhotoDto dto)
    {
        var photoLink = await cosmosDbContext.MarkerPhotos
            .FirstOrDefaultAsync(pl => pl.PhotoLinkId == dto.PhotoLinkId);

        if (photoLink == null)
            return NotFound();

        var photo = photoLink.Photos.FirstOrDefault(p => p.Id == dto.PhotoData.Id);

        if (photo == null)
            return NotFound();

        photo.Legend = dto.PhotoData.Legend;

        await cosmosDbContext.SaveChangesAsync();

        return Ok(photo);
    }

    [HttpDelete]
    public async Task<IActionResult> DeletePhoto([FromBody] DeletePhotoDto dto)
    {
        var photoLink = await cosmosDbContext.MarkerPhotos
            .FirstOrDefaultAsync(pl => pl.PhotoLinkId == dto.PhotoLinkId);

        if (photoLink == null)
            return NotFound();

        photoLink.Photos = photoLink.Photos.Where(p => p.Id != dto.PhotoId).ToList();

        await cosmosDbContext.SaveChangesAsync();

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

            var response = await s3Client.DeleteObjectAsync(deleteRequest);

            return Ok($"Deleted '{key}' from bucket '{bucketName}'.");
        }
        catch (System.Exception e)
        {
            System.Console.WriteLine(e);
            throw;
        }
    }
}